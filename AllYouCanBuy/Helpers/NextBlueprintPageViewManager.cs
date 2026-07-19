using System.Reflection;
using AllYouCanBuy.Systems;
using AllYouCanBuy.Views;
using Kitchen;
using KitchenMods;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    public class NextBlueprintPageViewManager : IModInitializer
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
            _manager.AddComponent<NextBlueprintPageViewManagerBehaviour>();
        }
    }

    public class NextBlueprintPageViewManagerBehaviour : MonoBehaviour
    {
        private const float UpdateInterval = 0.1f;
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
            var hasNextPageView = false;

            foreach (var rerollView in rerollViews)
            {
                var title = TitleField?.GetValue(rerollView) as TextMeshPro;
                var titleText = title?.text ?? string.Empty;
                var nextPageView = rerollView.GetComponent<NextBlueprintPageView>();
                var hasZeroPrice = GetHasZeroPrice(titleText);
                var hasPageTitle = nextPageView?.IsNextPage == true && NextBlueprintPageView.IsPageTitle(titleText);
                var isNextPage = hasPageTitle || (hasZeroPrice ?? (nextPageView?.IsNextPage == true));

                if (isNextPage && nextPageView == null)
                {
                    nextPageView = rerollView.gameObject.AddComponent<NextBlueprintPageView>();
                }

                nextPageView?.UpdateState(rerollView, isNextPage);
                hasNextPageView |= isNextPage;
            }

            UpdateProgressViews(hasNextPageView);
        }

        internal static bool? GetHasZeroPrice(string title)
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

        private static void UpdateProgressViews(bool hasNextPageView)
        {
            var targetView = GetNextPageProgressView();
            foreach (var progressView in Object.FindObjectsOfType<ProgressView>())
            {
                var isNextPage = hasNextPageView && progressView == targetView;
                var nextPageView = progressView.GetComponent<NextBlueprintPageProgressView>();

                if (isNextPage && nextPageView == null)
                {
                    nextPageView = progressView.gameObject.AddComponent<NextBlueprintPageProgressView>();
                }

                nextPageView?.UpdateState(progressView, isNextPage);
            }
        }

        private static ProgressView? GetNextPageProgressView()
        {
            var identifier = SetNextBlueprintPageCost.ProgressViewIdentifier;
            var viewManager = World.DefaultGameObjectInjectionWorld?
                .GetExistingSystem<EntityViewManager>();
            if (identifier == 0 || viewManager == null ||
                !viewManager.EntityViews.TryGetValue(
                    new ViewIdentifier { Identifier = identifier },
                    out var view
                ))
            {
                return null;
            }

            return view as ProgressView;
        }
    }
}
