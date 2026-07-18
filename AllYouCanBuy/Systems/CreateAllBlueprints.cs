using System.Linq;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace AllYouCanBuy.Systems
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    public class CreateAllBlueprints : ShopSystem, IModSystem
    {
        private EntityQuery _discounts;

        protected override void Initialise()
        {
            base.Initialise();
            _discounts = GetEntityQuery(typeof(CGrantsShopDiscount));
        }

        protected override void OnShopUpdate()
        {
            var freeTiles = BlueprintPageHelper.FindFreeTiles(
                Bounds,
                GetNameplateTile(),
                GetRerollTile(),
                GetPracticeTile()
            );

            var priceMultiplier = _discounts // Apply all discounts.
                .ToComponentDataArray<CGrantsShopDiscount>(Allocator.Temp)
                .Aggregate(1f, (current, discount) => current * (1f - discount.Amount));

            BlueprintPageHelper.Create(this, freeTiles, priceMultiplier);
        }
    }
}
