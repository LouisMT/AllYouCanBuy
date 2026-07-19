using AllYouCanBuy.Views;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace AllYouCanBuy.Systems
{
    [UpdateAfter(typeof(RerollBlueprintView.UpdateRerollBlueprintView))]
    [UpdateAfter(typeof(ProgressView.UpdateProgressView))]
    public class UpdateCycleBlueprintViews : ViewSystemBase, IModSystem
    {
        private EntityQuery _rerollTiles;
        private EntityQuery _progressIndicators;

        protected override void Initialise()
        {
            base.Initialise();
            _rerollTiles = GetEntityQuery(typeof(CRerollShopAfterDuration), typeof(CLinkedView));
            _progressIndicators = GetEntityQuery(typeof(CIndicator), typeof(CProgressIndicator), typeof(CLinkedView));
        }

        protected override void OnUpdate()
        {
            var isCycle = HasSingleton<SDay>() && GetSingleton<SDay>().Day % 5 != 0;

            using var rerollViews = _rerollTiles.ToComponentDataArray<CLinkedView>(Allocator.Temp);
            foreach (var rerollView in rerollViews)
            {
                SendUpdate(rerollView.Identifier, new NextBlueprintPageView.ViewData { IsCycle = isCycle });
            }

            using var indicators = _progressIndicators.ToComponentDataArray<CIndicator>(Allocator.Temp);
            using var progressViews = _progressIndicators.ToComponentDataArray<CLinkedView>(Allocator.Temp);
            for (var i = 0; i < indicators.Length; i++)
            {
                if (Has<CRerollShopAfterDuration>(indicators[i].Source))
                {
                    SendUpdate(
                        progressViews[i].Identifier,
                        new CycleBlueprintProgressView.ViewData { IsCycle = isCycle }
                    );
                }
            }
        }
    }
}
