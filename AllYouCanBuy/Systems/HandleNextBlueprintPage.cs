using System.Linq;
using AllYouCanBuy.Helpers;
using AllYouCanBuy.Views;
using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy.Systems
{
    [UpdateAfter(typeof(RerollShopAfterDuration))]
    [UpdateBefore(typeof(HandleShopReroll))]
    [UpdateBefore(typeof(RerollBlueprintView.UpdateRerollBlueprintView))]
    public class HandleNextBlueprintPage : GameSystemBase, IModSystem
    {
        private EntityQuery _requests;
        private EntityQuery _blueprints;
        private EntityQuery _discounts;
        private EntityQuery _progressIndicators;
        private string? _rerollLabel;

        protected override void Initialise()
        {
            base.Initialise();
            _requests = GetEntityQuery(typeof(CShopRerollRequest));
            _blueprints = GetEntityQuery(typeof(CShopEntity), typeof(CApplianceBlueprint));
            _discounts = GetEntityQuery(typeof(CGrantsShopDiscount));
            _progressIndicators = GetEntityQuery(typeof(CIndicator), typeof(CProgressIndicator), typeof(CLinkedView));
        }

        protected override void OnUpdate()
        {
            var isPagingDay = HasSingleton<SDay>() && GetSingleton<SDay>().Day % 5 != 0;
            UpdatePromptLabel(isPagingDay);
            UpdateTileView(isPagingDay);
            UpdateProgressView(isPagingDay);

            if (!isPagingDay)
            {
                return;
            }

            if (_requests.IsEmpty)
            {
                return;
            }

            EntityManager.DestroyEntity(_requests);
            EntityManager.DestroyEntity(_blueprints);
            SetSingleton(new SRerollCost { Cost = 0 });

            var freeTiles = BlueprintPageHelper.FindFreeTiles(
                Bounds,
                GetNameplateTile(),
                GetRerollTile(),
                GetPracticeTile()
            );
            var priceMultiplier = _discounts
                .ToComponentDataArray<CGrantsShopDiscount>(Allocator.Temp)
                .Aggregate(1f, (current, discount) => current * (1f - discount.Amount));

            BlueprintPageHelper.Create(this, freeTiles, priceMultiplier);
        }

        private void UpdatePromptLabel(bool isPagingDay)
        {
            var localisation = GameData.Main?.GlobalLocalisation;
            if (localisation?.Text == null || !localisation.Text.TryGetValue("LABEL_REROLL", out var currentLabel))
            {
                return;
            }

            if (isPagingDay && currentLabel != NextBlueprintPageView.Label)
            {
                _rerollLabel = currentLabel;
                localisation.Text["LABEL_REROLL"] = NextBlueprintPageView.Label;
            }
            else if (!isPagingDay && _rerollLabel != null && currentLabel == NextBlueprintPageView.Label)
            {
                localisation.Text["LABEL_REROLL"] = _rerollLabel;
            }
        }

        private void UpdateTileView(bool isPagingDay)
        {
            foreach (var view in Object.FindObjectsOfType<RerollBlueprintView>())
            {
                var viewOverride = view.GetComponent<NextBlueprintPageView>();
                if (isPagingDay && viewOverride == null)
                {
                    view.gameObject.AddComponent<NextBlueprintPageView>().Initialise(view);
                }
                else if (!isPagingDay && viewOverride != null)
                {
                    viewOverride.enabled = false;
                    Object.Destroy(viewOverride);
                    view.UpdateData((IViewData)new RerollBlueprintView.ViewData
                    {
                        Price = GetOrDefault<SRerollCost>().Cost,
                        PlayerMoney = GetOrDefault<SMoney>().Amount
                    });
                }
            }
        }

        private void UpdateProgressView(bool isPagingDay)
        {
            if (!isPagingDay || _progressIndicators.IsEmpty)
            {
                return;
            }

            using var indicators = _progressIndicators.ToComponentDataArray<CIndicator>(Allocator.Temp);
            using var linkedViews = _progressIndicators.ToComponentDataArray<CLinkedView>(Allocator.Temp);

            for (var i = 0; i < indicators.Length; i++)
            {
                if (!Has<CRerollShopAfterDuration>(indicators[i].Source) ||
                    !EntityViewManager.EntityViews.TryGetValue(linkedViews[i].Identifier, out var objectView))
                {
                    continue;
                }

                // Look the ProgressView up via Unity directly rather than IObjectView.GetSubView:
                // the taste-test branch added a `skip_null_cache` parameter to GetSubView, so a call
                // built against one branch won't resolve on the other. GetComponentInChildren is the
                // same underlying lookup GetSubView performs and is stable across branches.
                var progressView = objectView as ProgressView
                                   ?? (objectView as Component)?.GetComponentInChildren<ProgressView>(true);
                if (progressView != null && progressView.GetComponent<CycleBlueprintProgressView>() == null)
                {
                    progressView.gameObject.AddComponent<CycleBlueprintProgressView>().Initialise(progressView);
                }
            }
        }
    }
}
