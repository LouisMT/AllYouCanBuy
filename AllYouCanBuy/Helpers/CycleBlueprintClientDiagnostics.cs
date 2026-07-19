using System.Reflection;
using AllYouCanBuy.Views;
using Kitchen;
using KitchenData;
using KitchenMods;
using TMPro;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    public class CycleBlueprintClientDiagnostics : IModInitializer
    {
        public void PostActivate(Mod mod)
        {
            var diagnostics = new GameObject("All You Can Buy Client Diagnostics");
            Object.DontDestroyOnLoad(diagnostics);
            diagnostics.AddComponent<CycleBlueprintClientDiagnosticsBehaviour>();
        }

        public void PreInject()
        {
        }

        public void PostInject()
        {
        }
    }

    internal class CycleBlueprintClientDiagnosticsBehaviour : MonoBehaviour
    {
        private static readonly FieldInfo? TitleField = typeof(RerollBlueprintView).GetField(
            "Title",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        private float _nextLogTime;
        private bool _loggedClientDetected;

        private void Update()
        {
            if (Time.unscaledTime < _nextLogTime)
            {
                return;
            }

            _nextLogTime = Time.unscaledTime + (_loggedClientDetected ? 5f : 1f);
            if (!CycleBlueprintClientLog.IsClient)
            {
                return;
            }

            if (!_loggedClientDetected)
            {
                _loggedClientDetected = true;
                var resources = string.Join(", ", typeof(CycleBlueprintClientDiagnostics).Assembly.GetManifestResourceNames());
                CycleBlueprintClientLog.Info(
                    $"Client diagnostics started; assembly={typeof(CycleBlueprintClientDiagnostics).Assembly.FullName}; " +
                    $"resources=[{resources}]"
                );
            }

            LogSnapshot();
        }

        private static void LogSnapshot()
        {
            var rerollViews = Object.FindObjectsOfType<RerollBlueprintView>();
            var cycleViews = Object.FindObjectsOfType<NextBlueprintPageView>();
            var progressViews = Object.FindObjectsOfType<ProgressView>();
            var cycleProgressViews = Object.FindObjectsOfType<CycleBlueprintProgressView>();
            var localisation = GameData.Main?.GlobalLocalisation;
            var rerollLabel = localisation?.Text != null &&
                              localisation.Text.TryGetValue("LABEL_REROLL", out var label)
                ? label
                : "<unavailable>";

            CycleBlueprintClientLog.Info(
                $"Snapshot: transports=[{CycleBlueprintClientLog.DescribeTransports()}]; " +
                $"rerollViews={rerollViews.Length}; cycleViews={cycleViews.Length}; " +
                $"progressViews={progressViews.Length}; cycleProgressViews={cycleProgressViews.Length}; " +
                $"LABEL_REROLL=\"{rerollLabel}\""
            );

            for (var i = 0; i < rerollViews.Length; i++)
            {
                var rerollView = rerollViews[i];
                var title = TitleField?.GetValue(rerollView) as TextMeshPro;
                var titleText = title?.text.Replace("\n", "\\n") ?? "<unavailable>";
                CycleBlueprintClientLog.Info(
                    $"RerollView[{i}]: name=\"{rerollView.name}\"; active={rerollView.gameObject.activeInHierarchy}; " +
                    $"title=\"{titleText}\"; cycleComponent={rerollView.GetComponent<NextBlueprintPageView>() != null}"
                );
            }
        }
    }
}
