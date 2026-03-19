using System.Collections.Generic;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    public class CreateAllBlueprints : ShopSystem, IModSystem
    {
        protected override void OnShopUpdate()
        {
            var freeTiles = FindFreeTiles();
            Debug.Log($"Found {freeTiles.Count} free tiles");

            using var applianceIds = ApplianceHelper.Main
                .CycleApplianceIds()
                .GetEnumerator();

            while (freeTiles.TryDequeue(out var freeTile) && applianceIds.MoveNext())
            {
                Debug.Log($"Spawning {applianceIds.Current} at {freeTile}");
                PostHelpers.CreateOpenedLetter(EntityManager, freeTile, applianceIds.Current);
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