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

        protected override void UpdateData(ViewData data)
        {
            _isCycle = data.IsCycle;
            UpdatePromptLabel();
            if (!_isCycle && !_initialised)
            {
                return;
            }

            if (!_initialised)
            {
                _initialised = true;
                var view = GetComponent<RerollBlueprintView>();
                _title = TitleField?.GetValue(view) as TextMeshPro;
                ReplaceDice(view.transform);
            }

            _dice?.SetActive(!_isCycle);
            _secondDice?.SetActive(!_isCycle);
            _icon?.SetActive(_isCycle);
        }

        private void UpdatePromptLabel()
        {
            var localisation = GameData.Main?.GlobalLocalisation;
            if (localisation?.Text == null || !localisation.Text.TryGetValue("LABEL_REROLL", out var currentLabel))
            {
                return;
            }

            if (_isCycle && currentLabel != Label)
            {
                _rerollLabel = currentLabel;
                localisation.Text["LABEL_REROLL"] = Label;
            }
            else if (!_isCycle && _rerollLabel != null && currentLabel == Label)
            {
                localisation.Text["LABEL_REROLL"] = _rerollLabel;
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
            }
        }

        protected override void OnDestroy()
        {
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
            var container = FindRerollContainer(start);
            if (container == null)
            {
                return;
            }

            _dice = container.Find("Dice")?.gameObject;
            _secondDice = container.Find("Dice (1)")?.gameObject;
            var diceRenderer = _dice?.GetComponent<Renderer>();
            if (diceRenderer == null || diceRenderer.sharedMaterials.Length == 0)
            {
                return;
            }

            _iconMesh = CycleBlueprintArrowsMesh.Create();
            _icon = new GameObject("Cycle Blueprint Arrows");
            _icon.transform.SetParent(container, false);
            _icon.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            _icon.transform.localScale = Vector3.one * 0.25f;
            _icon.AddComponent<MeshFilter>().sharedMesh = _iconMesh;
            _icon.AddComponent<MeshRenderer>().sharedMaterial = diceRenderer.sharedMaterials[0];

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
                var rerollView = (view as Component)?.GetComponentInChildren<RerollBlueprintView>(true);
                if (rerollView == null)
                {
                    return null!;
                }

                return rerollView.GetComponent<NextBlueprintPageView>()
                       ?? rerollView.gameObject.AddComponent<NextBlueprintPageView>();
            }

            public bool IsChangedFrom(ViewData check)
            {
                return IsCycle != check.IsCycle;
            }
        }
    }
}
