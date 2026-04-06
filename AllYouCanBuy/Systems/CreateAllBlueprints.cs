using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
            var freeTiles = FindFreeTiles();
            Logger.Info($"Found {freeTiles.Count} free tiles");

            var applianceIds = ApplianceHelper.CycleApplianceIds(this, freeTiles.Count);

            var priceMultiplier = _discounts // Apply all discounts.
                .ToComponentDataArray<CGrantsShopDiscount>(Allocator.Temp)
                .Aggregate(1f, (current, discount) => current * (1f - discount.Amount));

            for (var i = 0; i < freeTiles.Count; i++)
            {
                var freeTile = freeTiles[i];
                var applianceId = applianceIds[i];

                Logger.Info($"Spawning {applianceId} at {freeTile}");
                PostHelpers.CreateOpenedLetter(
                    EntityManager,
                    freeTile,
                    (int)applianceId,
                    priceMultiplier
                );
            }
        }

        private List<Vector3> FindFreeTiles()
        {
            var namePlate = GetNameplateTile();
            var reroll = GetRerollTile();
            var practice = GetPracticeTile();

            var tiles = new List<Vector3>();

            for (var x = Bounds.min.x; x <= Bounds.max.x; x++)
            {
                for (var z = reroll.z - 1; z <= reroll.z; z++)
                {
                    var position = new Vector3(x, 0, z);

                    if (position == namePlate || position == reroll || position == practice)
                    {
                        continue;
                    }

                    tiles.Add(position);
                }
            }

            return tiles;
        }
    }
}