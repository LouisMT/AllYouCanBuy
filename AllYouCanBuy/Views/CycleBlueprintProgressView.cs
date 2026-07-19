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

        protected override void UpdateData(ViewData data)
        {
            _isCycle = data.IsCycle;
            if (!_isCycle && !_initialised)
            {
                return;
            }

            if (!_initialised)
            {
                _initialised = true;
                var view = GetComponent<ProgressView>();
                _icon = IconField?.GetValue(view) as TextMeshPro;
                if (_icon == null)
                {
                    return;
                }

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
            }

            _spriteObject?.SetActive(_isCycle);
        }

        private void LateUpdate()
        {
            if (_isCycle && _icon != null)
            {
                _icon.text = string.Empty;
            }
        }

        protected override void OnDestroy()
        {
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
                var progressView = view as ProgressView
                                   ?? (view as Component)?.GetComponentInChildren<ProgressView>(true);
                if (progressView == null)
                {
                    return null!;
                }

                return progressView.GetComponent<CycleBlueprintProgressView>()
                       ?? progressView.gameObject.AddComponent<CycleBlueprintProgressView>();
            }

            public bool IsChangedFrom(ViewData check)
            {
                return IsCycle != check.IsCycle;
            }
        }
    }
}
