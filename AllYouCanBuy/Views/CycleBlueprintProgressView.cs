using System;
using System.Reflection;
using AllYouCanBuy.Helpers;
using Kitchen;
using MessagePack;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Views
{
    public class CycleBlueprintProgressView : UpdatableObjectView<CycleBlueprintProgressView.ViewData>
    {
        private static readonly FieldInfo? IconField = typeof(ProgressView).GetField(
            "Icon",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private TextMeshPro? _icon;
        private GameObject? _spriteObject;
        private Texture2D? _texture;
        private Sprite? _sprite;
        private bool _initialised;
        private bool _isCycle;
        private bool _loggedIconOverride;

        protected override void UpdateData(ViewData data)
        {
            CycleBlueprintClientLog.Info(
                $"CycleBlueprintProgressView.UpdateData received IsCycle={data.IsCycle}; " +
                $"initialised={_initialised}; object=\"{name}\""
            );
            _isCycle = data.IsCycle;
            if (!_isCycle && !_initialised)
            {
                CycleBlueprintClientLog.Info("Skipping progress icon initialisation because this is a normal reroll day");
                return;
            }

            if (!_initialised)
            {
                _initialised = true;
                var view = GetComponent<ProgressView>();
                _icon = IconField?.GetValue(view) as TextMeshPro;
                CycleBlueprintClientLog.Info(
                    $"Initialising cycle progress view; progressViewFound={view != null}; iconTextFound={_icon != null}; " +
                    $"iconText=\"{_icon?.text ?? "<unavailable>"}\""
                );
                if (_icon == null)
                {
                    CycleBlueprintClientLog.Warning("Cannot initialise cycle progress icon because ProgressView.Icon was not found");
                    return;
                }

                try
                {
                    _texture = CycleBlueprintArrowsIcon.CreateTexture();
                    _sprite = CycleBlueprintArrowsIcon.CreateSprite(_texture);
                    _spriteObject = new GameObject("Cycle Blueprint Arrows Icon");
                    _spriteObject.transform.SetParent(_icon.transform, false);
                    _spriteObject.transform.localScale = Vector3.one * 4f;

                    var spriteRenderer = _spriteObject.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = _sprite;

                    var textRenderer = _icon.GetComponent<Renderer>();
                    if (textRenderer != null)
                    {
                        spriteRenderer.sortingLayerID = textRenderer.sortingLayerID;
                        spriteRenderer.sortingOrder = textRenderer.sortingOrder + 1;
                    }

                    CycleBlueprintClientLog.Info(
                        $"Created cycle progress icon; texture={_texture.width}x{_texture.height}; " +
                        $"textRendererFound={textRenderer != null}; sortingOrder={spriteRenderer.sortingOrder}"
                    );
                }
                catch (Exception exception)
                {
                    CycleBlueprintClientLog.Error("Failed to create cycle progress icon", exception);
                    throw;
                }
            }

            _spriteObject?.SetActive(_isCycle);
            _loggedIconOverride = false;
            CycleBlueprintClientLog.Info(
                $"Applied cycle progress state; isCycle={_isCycle}; spriteFound={_spriteObject != null}; " +
                $"spriteActive={_spriteObject?.activeSelf}"
            );
        }

        private void LateUpdate()
        {
            if (_isCycle && _icon != null)
            {
                _icon.text = string.Empty;
                if (!_loggedIconOverride)
                {
                    _loggedIconOverride = true;
                    CycleBlueprintClientLog.Info("LateUpdate replaced the default progress text with the cycle icon");
                }
            }
        }

        protected override void OnDestroy()
        {
            CycleBlueprintClientLog.Info(
                $"Destroying CycleBlueprintProgressView; initialised={_initialised}; isCycle={_isCycle}; object=\"{name}\""
            );
            if (_spriteObject != null)
            {
                Destroy(_spriteObject);
            }

            if (_sprite != null)
            {
                Destroy(_sprite);
            }

            if (_texture != null)
            {
                Destroy(_texture);
            }

            base.OnDestroy();
        }

        [Serializable, MessagePackObject(false)]
        public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public bool IsCycle;

            public IUpdatableObject GetRelevantSubview(IObjectView view)
            {
                CycleBlueprintClientLog.Info(
                    $"Progress ViewData.GetRelevantSubview received IsCycle={IsCycle}; rootType={view.GetType().FullName}"
                );
                var progressView = view as ProgressView
                                   ?? (view as Component)?.GetComponentInChildren<ProgressView>(true);
                if (progressView == null)
                {
                    CycleBlueprintClientLog.Warning("Progress ViewData could not find ProgressView below the routed root view");
                    return null!;
                }

                var cycleView = progressView.GetComponent<CycleBlueprintProgressView>();
                CycleBlueprintClientLog.Info(
                    $"Progress ViewData found object=\"{progressView.name}\"; existingCycleComponent={cycleView != null}"
                );
                return cycleView ?? progressView.gameObject.AddComponent<CycleBlueprintProgressView>();
            }

            public bool IsChangedFrom(ViewData check)
            {
                return IsCycle != check.IsCycle;
            }
        }
    }
}
