using System.Collections.Generic;
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
        public void PostActivate(Mod mod)
        {
            var manager = new GameObject("All You Can Buy View Manager");
            Object.DontDestroyOnLoad(manager);
            manager.AddComponent<CycleBlueprintViewManagerBehaviour>();
            Logger.Info("[CycleBlueprintViewManager] Runtime view manager installed");
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
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

        private readonly Dictionary<int, string> _lastTitles = new Dictionary<int, string>();
        private float _nextUpdateTime;
        private int _lastRerollViewCount = -1;

        private void Update()
        {
            if (Time.unscaledTime < _nextUpdateTime)
            {
                return;
            }

            _nextUpdateTime = Time.unscaledTime + UpdateInterval;
            var rerollViews = Object.FindObjectsOfType<RerollBlueprintView>();
            var hasCycleView = false;

            if (rerollViews.Length != _lastRerollViewCount)
            {
                _lastRerollViewCount = rerollViews.Length;
                Logger.Info($"[CycleBlueprintViewManager] Found {rerollViews.Length} reroll views");
            }

            foreach (var rerollView in rerollViews)
            {
                var title = TitleField?.GetValue(rerollView) as TextMeshPro;
                var titleText = title?.text ?? string.Empty;
                var isCycle = HasZeroPrice(titleText);
                var cycleView = rerollView.GetComponent<NextBlueprintPageView>();
                var viewId = rerollView.GetInstanceID();

                if (!_lastTitles.TryGetValue(viewId, out var previousTitle) || previousTitle != titleText)
                {
                    _lastTitles[viewId] = titleText;
                    Logger.Info(
                        $"[CycleBlueprintViewManager] Reroll title=\"{titleText.Replace("\n", "\\n")}\"; " +
                        $"zeroPrice={isCycle}; titleFieldFound={title != null}"
                    );
                }

                if (isCycle && cycleView == null)
                {
                    cycleView = rerollView.gameObject.AddComponent<NextBlueprintPageView>();
                    Logger.Info($"[CycleBlueprintViewManager] Detected free reroll view {rerollView.name}; enabling cycle visuals");
                }

                cycleView?.UpdateState(rerollView, isCycle);
                hasCycleView |= isCycle;
            }

            UpdateProgressViews(rerollViews, hasCycleView);
        }

        private static bool HasZeroPrice(string title)
        {
            var lineBreak = title.LastIndexOf('\n');
            if (lineBreak < 0)
            {
                return false;
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

            return false;
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
                    Logger.Info($"[CycleBlueprintViewManager] Detected cycle progress view {progressView.name}");
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
