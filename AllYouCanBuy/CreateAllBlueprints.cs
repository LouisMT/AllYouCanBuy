using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy
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
            Debug.Log($"Found {freeTiles.Count} free tiles");

            using var applianceIds = ApplianceHelper.Main
                .CycleApplianceIds()
                .GetEnumerator();

            var priceMultiplier = _discounts // Apply all discounts.
                .ToComponentDataArray<CGrantsShopDiscount>(Allocator.Temp)
                .Aggregate(1f, (current, discount) => current * (1f - discount.Amount));

            while (freeTiles.TryDequeue(out var freeTile) && applianceIds.MoveNext())
            {
                Debug.Log($"Spawning {applianceIds.Current} at {freeTile}");
                PostHelpers.CreateOpenedLetter(
                    EntityManager,
                    freeTile,
                    applianceIds.Current,
                    priceMultiplier
                );
            }
        }

        private Queue<Vector3> FindFreeTiles()
        {
            var namePlate = GetNameplateTile();
            var reroll = GetRerollTile();
            var practice = GetPracticeTile();

            var tiles = new Queue<Vector3>();

            for (var x = Bounds.min.x; x <= Bounds.max.x; x++)
            {
                for (var z = reroll.z - 1; z <= reroll.z; z++)
                {
                    var position = new Vector3(x, 0, z);

                    if (position == namePlate || position == reroll || position == practice)
                    {
                        continue;
                    }

                    tiles.Enqueue(position);
                }
            }

            return tiles;
        }
    }
}