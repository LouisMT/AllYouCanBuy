using System;
using System.Reflection;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenData;
using MessagePack;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Views
{
    public class NextBlueprintPageView : UpdatableObjectView<NextBlueprintPageView.ViewData>
    {
        public const string Label = "Cycle Blueprints";

        private static readonly FieldInfo? TitleField = typeof(RerollBlueprintView).GetField(
            "Title",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        private static string? _rerollLabel;

        private TextMeshPro? _title;
        private GameObject? _dice;
        private GameObject? _secondDice;
        private GameObject? _icon;
        private Mesh? _iconMesh;
        private bool _initialised;
        private bool _isCycle;
        private bool _loggedTitleOverride;

        protected override void UpdateData(ViewData data)
        {
            CycleBlueprintClientLog.Info(
                $"NextBlueprintPageView.UpdateData received IsCycle={data.IsCycle}; " +
                $"initialised={_initialised}; object=\"{name}\""
            );
            _isCycle = data.IsCycle;
            UpdatePromptLabel();
            if (!_isCycle && !_initialised)
            {
                CycleBlueprintClientLog.Info("Skipping tile asset initialisation because this is a normal reroll day");
                return;
            }

            if (!_initialised)
            {
                var view = GetComponent<RerollBlueprintView>();
                if (ReferenceEquals(view, null))
                {
                    CycleBlueprintClientLog.Warning("Cannot initialise cycle tile because RerollBlueprintView is missing");
                    return;
                }

                _initialised = true;
                _title = TitleField?.GetValue(view) as TextMeshPro;
                CycleBlueprintClientLog.Info(
                    $"Initialising cycle tile; rerollViewFound=true; titleFound={_title != null}; " +
                    $"title=\"{_title?.text.Replace("\n", "\\n") ?? "<unavailable>"}\""
                );

                try
                {
                    ReplaceDice(view.transform);
                }
                catch (Exception exception)
                {
                    CycleBlueprintClientLog.Error("Failed to replace reroll dice", exception);
                    throw;
                }
            }

            _dice?.SetActive(!_isCycle);
            _secondDice?.SetActive(!_isCycle);
            _icon?.SetActive(_isCycle);
            _loggedTitleOverride = false;
            CycleBlueprintClientLog.Info(
                $"Applied cycle tile state; isCycle={_isCycle}; diceFound={_dice != null}; " +
                $"secondDiceFound={_secondDice != null}; iconFound={_icon != null}; iconActive={_icon?.activeSelf}"
            );
        }

        private void UpdatePromptLabel()
        {
            var localisation = GameData.Main?.GlobalLocalisation;
            if (localisation?.Text == null || !localisation.Text.TryGetValue("LABEL_REROLL", out var currentLabel))
            {
                CycleBlueprintClientLog.Warning("Could not read LABEL_REROLL from global localisation");
                return;
            }

            CycleBlueprintClientLog.Info(
                $"Updating prompt label; isCycle={_isCycle}; current=\"{currentLabel}\"; " +
                $"saved=\"{_rerollLabel ?? "<null>"}\""
            );

            if (_isCycle && currentLabel != Label)
            {
                _rerollLabel = currentLabel;
                localisation.Text["LABEL_REROLL"] = Label;
                CycleBlueprintClientLog.Info($"Changed LABEL_REROLL from \"{currentLabel}\" to \"{Label}\"");
            }
            else if (!_isCycle && _rerollLabel != null && currentLabel == Label)
            {
                localisation.Text["LABEL_REROLL"] = _rerollLabel;
                CycleBlueprintClientLog.Info($"Restored LABEL_REROLL to \"{_rerollLabel}\"");
                if (_title != null)
                {
                    var lineBreak = _title.text.IndexOf('\n');
                    var priceLine = lineBreak >= 0
                        ? _title.text.Substring(lineBreak)
                        : string.Empty;
                    _title.text = _rerollLabel + priceLine;
                }
            }
        }

        private void LateUpdate()
        {
            if (_isCycle && _title != null)
            {
                _title.text = Label;
                if (!_loggedTitleOverride)
                {
                    _loggedTitleOverride = true;
                    CycleBlueprintClientLog.Info("LateUpdate applied the Cycle Blueprints tile title");
                }
            }
        }

        protected override void OnDestroy()
        {
            CycleBlueprintClientLog.Info(
                $"Destroying NextBlueprintPageView; initialised={_initialised}; isCycle={_isCycle}; object=\"{name}\""
            );
            if (_dice != null)
            {
                _dice.SetActive(true);
            }

            if (_secondDice != null)
            {
                _secondDice.SetActive(true);
            }

            if (_icon != null)
            {
                Destroy(_icon);
            }

            if (_iconMesh != null)
            {
                Destroy(_iconMesh);
            }

            base.OnDestroy();
        }

        private void ReplaceDice(Transform start)
        {
            CycleBlueprintClientLog.Info($"Searching for reroll dice from transform=\"{start.name}\"");
            var container = FindRerollContainer(start);
            if (container == null)
            {
                CycleBlueprintClientLog.Warning("Could not find a reroll Container with a Dice child");
                return;
            }

            _dice = container.Find("Dice")?.gameObject;
            _secondDice = container.Find("Dice (1)")?.gameObject;
            var diceRenderer = _dice?.GetComponent<Renderer>();
            CycleBlueprintClientLog.Info(
                $"Found reroll container=\"{container.name}\"; dice={_dice != null}; " +
                $"secondDice={_secondDice != null}; renderer={diceRenderer != null}; " +
                $"materials={diceRenderer?.sharedMaterials.Length ?? 0}"
            );
            if (diceRenderer == null || diceRenderer.sharedMaterials.Length == 0)
            {
                CycleBlueprintClientLog.Warning("Cannot create cycle mesh because the reroll dice renderer or material is missing");
                return;
            }

            _iconMesh = CycleBlueprintArrowsMesh.Create();
            _icon = new GameObject("Cycle Blueprint Arrows");
            _icon.transform.SetParent(container, false);
            _icon.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            _icon.transform.localScale = Vector3.one * 0.25f;
            _icon.AddComponent<MeshFilter>().sharedMesh = _iconMesh;
            _icon.AddComponent<MeshRenderer>().sharedMaterial = diceRenderer.sharedMaterials[0];
            CycleBlueprintClientLog.Info(
                $"Created cycle mesh object; vertices={_iconMesh.vertexCount}; " +
                $"position={_icon.transform.localPosition}; scale={_icon.transform.localScale}"
            );

            _dice?.SetActive(false);
            _secondDice?.SetActive(false);
        }

        private static Transform? FindRerollContainer(Transform current)
        {
            while (current != null)
            {
                var container = current.Find("Container");
                if (container != null && container.Find("Dice") != null)
                {
                    return container;
                }

                current = current.parent;
            }

            return null;
        }

        [Serializable, MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool IsCycle;

            public IUpdatableObject GetRelevantSubview(IObjectView view)
            {
                CycleBlueprintClientLog.Info(
                    $"Tile ViewData.GetRelevantSubview received IsCycle={IsCycle}; rootType={view.GetType().FullName}"
                );
                var rerollView = (view as Component)?.GetComponentInChildren<RerollBlueprintView>(true);
                if (rerollView == null)
                {
                    CycleBlueprintClientLog.Warning("Tile ViewData could not find RerollBlueprintView below the routed root view");
                    return null!;
                }

                var cycleView = rerollView.GetComponent<NextBlueprintPageView>();
                CycleBlueprintClientLog.Info(
                    $"Tile ViewData found reroll object=\"{rerollView.name}\"; existingCycleComponent={cycleView != null}"
                );
                return cycleView ?? rerollView.gameObject.AddComponent<NextBlueprintPageView>();
            }

            public bool IsChangedFrom(ViewData check)
            {
                return IsCycle != check.IsCycle;
            }
        }
    }
}
