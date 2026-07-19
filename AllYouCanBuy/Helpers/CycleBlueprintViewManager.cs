using System.Reflection;
using AllYouCanBuy.Views;
using Kitchen;
using KitchenMods;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    public class CycleBlueprintViewManager : IModInitializer
    {
        private static GameObject? _manager;

        public void PostActivate(Mod mod)
        {
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
            if (_manager != null)
            {
                return;
            }

            _manager = new GameObject("All You Can Buy View Manager");
            Object.DontDestroyOnLoad(_manager);
            _manager.AddComponent<CycleBlueprintViewManagerBehaviour>();
        }
    }

    public class CycleBlueprintViewManagerBehaviour : MonoBehaviour
    {
        private const float UpdateInterval = 0.1f;
        private const float ProgressViewRangeSquared = 4f;

        private static readonly FieldInfo? TitleField = typeof(RerollBlueprintView).GetField(
            "Title",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private float _nextUpdateTime;

        private void Update()
        {
            if (Time.unscaledTime < _nextUpdateTime)
            {
                return;
            }

            _nextUpdateTime = Time.unscaledTime + UpdateInterval;
            var rerollViews = Object.FindObjectsOfType<RerollBlueprintView>();
            var hasCycleView = false;

            foreach (var rerollView in rerollViews)
            {
                var title = TitleField?.GetValue(rerollView) as TextMeshPro;
                var titleText = title?.text ?? string.Empty;
                var cycleView = rerollView.GetComponent<NextBlueprintPageView>();
                var hasZeroPrice = GetHasZeroPrice(titleText);
                var isCycle = hasZeroPrice ?? (cycleView?.IsCycle == true);

                if (isCycle && cycleView == null)
                {
                    cycleView = rerollView.gameObject.AddComponent<NextBlueprintPageView>();
                }

                cycleView?.UpdateState(rerollView, isCycle);
                hasCycleView |= isCycle;
            }

            UpdateProgressViews(rerollViews, hasCycleView);
        }

        private static bool? GetHasZeroPrice(string title)
        {
            var lineBreak = title.LastIndexOf('\n');
            if (lineBreak < 0)
            {
                return null;
            }

            var insideTag = false;
            for (var i = lineBreak + 1; i < title.Length; i++)
            {
                switch (title[i])
                {
                    case '<':
                        insideTag = true;
                        break;
                    case '>':
                        insideTag = false;
                        break;
                    default:
                        if (!insideTag && !char.IsWhiteSpace(title[i]))
                        {
                            return title[i] == '0';
                        }

                        break;
                }
            }

            return null;
        }

        private static void UpdateProgressViews(RerollBlueprintView[] rerollViews, bool hasCycleView)
        {
            foreach (var progressView in Object.FindObjectsOfType<ProgressView>())
            {
                var isCycle = hasCycleView && IsNearRerollView(progressView.transform.position, rerollViews);
                var cycleView = progressView.GetComponent<CycleBlueprintProgressView>();

                if (isCycle && cycleView == null)
                {
                    cycleView = progressView.gameObject.AddComponent<CycleBlueprintProgressView>();
                }

                cycleView?.UpdateState(progressView, isCycle);
            }
        }

        private static bool IsNearRerollView(Vector3 position, RerollBlueprintView[] rerollViews)
        {
            foreach (var rerollView in rerollViews)
            {
                if ((rerollView.transform.position - position).sqrMagnitude <= ProgressViewRangeSquared)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
