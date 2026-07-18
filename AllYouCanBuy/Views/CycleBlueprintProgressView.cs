using System.Reflection;
using AllYouCanBuy.Helpers;
using Kitchen;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Views
{
    public class CycleBlueprintProgressView : MonoBehaviour
    {
        private static readonly FieldInfo? IconField = typeof(ProgressView).GetField(
            "Icon",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private TextMeshPro? _icon;
        private GameObject? _spriteObject;
        private Texture2D? _texture;
        private Sprite? _sprite;

        public void Initialise(ProgressView view)
        {
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

        private void LateUpdate()
        {
            if (_icon != null)
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
