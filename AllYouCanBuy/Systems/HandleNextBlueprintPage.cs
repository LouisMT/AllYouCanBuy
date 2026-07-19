using System.Linq;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

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

        protected override void Initialise()
        {
            base.Initialise();
            _requests = GetEntityQuery(typeof(CShopRerollRequest));
            _blueprints = GetEntityQuery(typeof(CShopEntity), typeof(CApplianceBlueprint));
            _discounts = GetEntityQuery(typeof(CGrantsShopDiscount));
        }

        protected override void OnUpdate()
        {
            var isPagingDay = HasSingleton<SDay>() && GetSingleton<SDay>().Day % 5 != 0;

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
    }
}
