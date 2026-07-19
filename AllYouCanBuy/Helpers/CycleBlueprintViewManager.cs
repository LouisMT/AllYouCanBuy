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
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
        }
    }

    internal class CycleBlueprintViewManagerBehaviour : MonoBehaviour
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
                var isCycle = title?.text.Contains("\n<size=2.4>0</size>") == true;
                var cycleView = rerollView.GetComponent<NextBlueprintPageView>();

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
