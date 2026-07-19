using System.Reflection;
using AllYouCanBuy.Helpers;
using Kitchen;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Views
{
    public class NextBlueprintPageProgressView : MonoBehaviour
    {
        private static readonly FieldInfo? IconField = typeof(ProgressView).GetField(
            "Icon",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private TextMeshPro? _icon;
        private GameObject? _spriteObject;
        private Texture2D? _texture;
        private Sprite? _sprite;
        private string? _rerollIcon;
        private bool _initialised;
        private bool _isNextPage;

        internal void UpdateState(ProgressView view, bool isNextPage)
        {
            var wasNextPage = _isNextPage;
            _isNextPage = isNextPage;
            if (!_initialised && _isNextPage)
            {
                _initialised = true;
                _icon = IconField?.GetValue(view) as TextMeshPro;
                if (_icon == null)
                {
                    return;
                }

                _rerollIcon = _icon.text;
                _texture = NextPageCircledChevronIcon.CreateTexture();
                _sprite = NextPageCircledChevronIcon.CreateSprite(_texture);
                _spriteObject = new GameObject("Next Page Circled Chevron Icon");
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

            _spriteObject?.SetActive(_isNextPage);
            if (wasNextPage && !_isNextPage && _icon != null && _rerollIcon != null)
            {
                _icon.text = _rerollIcon;
            }
        }

        private void LateUpdate()
        {
            if (_isNextPage && _icon != null)
            {
                _icon.text = string.Empty;
            }
        }

        private void OnDestroy()
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
        }
    }
}
