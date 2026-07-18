using System.Reflection;
using AllYouCanBuy.Helpers;
using Kitchen;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Views
{
    public class NextBlueprintPageView : MonoBehaviour
    {
        public const string Label = "Cycle Blueprints";

        private static readonly FieldInfo? TitleField = typeof(RerollBlueprintView).GetField(
            "Title",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private TextMeshPro? _title;
        private GameObject? _dice;
        private GameObject? _secondDice;
        private GameObject? _icon;
        private Mesh? _iconMesh;

        public void Initialise(RerollBlueprintView view)
        {
            _title = TitleField?.GetValue(view) as TextMeshPro;
            ReplaceDice(view.transform);
        }

        private void LateUpdate()
        {
            if (_title != null)
            {
                _title.text = Label;
            }
        }

        private void OnDestroy()
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
    }
}
