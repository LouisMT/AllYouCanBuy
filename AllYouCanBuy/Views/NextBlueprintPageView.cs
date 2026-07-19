using System.Reflection;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenData;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Views
{
    public class NextBlueprintPageView : MonoBehaviour
    {
        public const string Label = "Next Page";
        private const float IconCameraInclineDegrees = 25f;

        private static readonly FieldInfo? TitleField = typeof(RerollBlueprintView).GetField(
            "Title",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        private static readonly Vector3 IconFlipAxis = new Vector3(
            0f,
            -Mathf.Sin(IconCameraInclineDegrees * Mathf.Deg2Rad),
            Mathf.Cos(IconCameraInclineDegrees * Mathf.Deg2Rad)
        );
        private static readonly Vector3 IconSpinAxis = new Vector3(
            0f,
            Mathf.Cos(IconCameraInclineDegrees * Mathf.Deg2Rad),
            Mathf.Sin(IconCameraInclineDegrees * Mathf.Deg2Rad)
        );
        private static string? _rerollLabel;

        private TextMeshPro? _title;
        private GameObject? _dice;
        private GameObject? _secondDice;
        private GameObject? _icon;
        private Mesh? _iconMesh;
        private Transform? _iconContainer;
        private Quaternion _iconBaseWorldRotation;
        private Quaternion _iconContainerBaseRotation;
        private bool _initialised;
        private bool _isIconFlipped;
        private bool _isNextPage;

        internal bool IsNextPage => _isNextPage;

        internal void UpdateState(RerollBlueprintView view, bool isNextPage)
        {
            _isNextPage = isNextPage;
            if (!_initialised && _isNextPage)
            {
                _initialised = true;
                _title = TitleField?.GetValue(view) as TextMeshPro;
                ReplaceDice(view.transform);
            }

            UpdatePromptLabel();
            _dice?.SetActive(!_isNextPage);
            _secondDice?.SetActive(!_isNextPage);
            _icon?.SetActive(_isNextPage);
        }

        internal static bool IsPageTitle(string title)
        {
            var prefix = Label + "\n";
            if (!title.StartsWith(prefix))
            {
                return false;
            }

            var separator = title.IndexOf('/', prefix.Length);
            return separator > prefix.Length &&
                separator == title.LastIndexOf('/') &&
                int.TryParse(title.Substring(prefix.Length, separator - prefix.Length), out var currentPage) &&
                int.TryParse(title.Substring(separator + 1), out var pageCount) &&
                currentPage > 0 && currentPage <= pageCount;
        }

        private void UpdatePromptLabel()
        {
            var localisation = GameData.Main?.GlobalLocalisation;
            if (localisation?.Text == null || !localisation.Text.TryGetValue("LABEL_REROLL", out var currentLabel))
            {
                return;
            }

            if (_isNextPage && currentLabel != Label)
            {
                _rerollLabel = currentLabel;
                localisation.Text["LABEL_REROLL"] = Label;
            }
            else if (!_isNextPage && _rerollLabel != null && currentLabel == Label)
            {
                localisation.Text["LABEL_REROLL"] = _rerollLabel;
                if (_title != null)
                {
                    var lineBreak = _title.text.IndexOf('\n');
                    var suffix = !IsPageTitle(_title.text) && lineBreak >= 0
                        ? _title.text.Substring(lineBreak)
                        : string.Empty;
                    _title.text = _rerollLabel + suffix;
                }
            }
        }

        private void LateUpdate()
        {
            if (!_isNextPage)
            {
                return;
            }

            UpdateIconRotation();
            if (_title == null)
            {
                return;
            }

            if (!IsPageTitle(_title.text) &&
                NextBlueprintPageViewManagerBehaviour.GetHasZeroPrice(_title.text) == false)
            {
                return;
            }

            _title.text = GetPageTitle();
        }

        private void UpdateIconRotation()
        {
            var camera = Camera.main;
            if (_icon == null || _iconContainer == null || camera == null)
            {
                return;
            }

            var containerRotation = Quaternion.Inverse(_iconContainerBaseRotation) *
                _iconContainer.rotation;
            var containerForward = containerRotation * Vector3.forward;
            var spinDegrees = Mathf.Atan2(containerForward.x, containerForward.z) *
                Mathf.Rad2Deg;
            ApplyIconRotation(spinDegrees);

            var screenDirection = Vector3.Dot(
                _icon.transform.TransformDirection(Vector3.right),
                camera.transform.right
            );
            if (screenDirection >= -0.01f)
            {
                return;
            }

            _isIconFlipped = !_isIconFlipped;
            ApplyIconRotation(spinDegrees);
        }

        private void ApplyIconRotation(float spinDegrees)
        {
            if (_icon == null)
            {
                return;
            }

            var flip = _isIconFlipped
                ? Quaternion.AngleAxis(180f, IconFlipAxis)
                : Quaternion.identity;
            _icon.transform.rotation = _iconBaseWorldRotation *
                Quaternion.AngleAxis(spinDegrees, IconSpinAxis) *
                flip;
        }

        private void OnDestroy()
        {
            if (_isNextPage)
            {
                _isNextPage = false;
                UpdatePromptLabel();
            }

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

            _iconMesh = NextPageCircledChevronMesh.Create();
            _icon = new GameObject("Next Page Circled Chevron");
            _icon.transform.SetParent(container, false);
            _icon.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            _icon.transform.localScale = Vector3.one * 0.3f;
            _iconContainer = container;
            _iconContainerBaseRotation = container.rotation;
            _iconBaseWorldRotation = _icon.transform.rotation;
            _icon.AddComponent<MeshFilter>().sharedMesh = _iconMesh;
            _icon.AddComponent<MeshRenderer>().sharedMaterial = diceRenderer.sharedMaterials[0];

            _dice?.SetActive(false);
            _secondDice?.SetActive(false);
        }

        private static string GetPageTitle()
        {
            return ApplianceHelper.PageCount > 0
                ? $"{Label}\n{ApplianceHelper.CurrentPage}/{ApplianceHelper.PageCount}"
                : Label;
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
