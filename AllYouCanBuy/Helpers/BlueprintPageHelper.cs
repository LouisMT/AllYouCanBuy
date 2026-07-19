using System.Collections.Generic;
using AllYouCanBuy.Extensions;
using Kitchen;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    internal static class BlueprintPageHelper
    {
        internal static List<Vector3> FindFreeTiles(
            Bounds bounds,
            Vector3 namePlate,
            Vector3 reroll,
            Vector3 practice)
        {
            var tiles = new List<Vector3>();

            for (var x = bounds.min.x; x <= bounds.max.x; x++)
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

        internal static void Create(GenericSystemBase system, IReadOnlyList<Vector3> freeTiles, float priceMultiplier)
        {
            Logger.Info($"Found {freeTiles.Count} free tiles");
            var applianceIds = ApplianceHelper.GetCurrentAppliancePage(freeTiles.Count);

            for (var i = 0; i < applianceIds.Length; i++)
            {
                var freeTile = freeTiles[i];
                var applianceId = applianceIds[i];

                Logger.Info($"Spawning {applianceId.ToName()} at {freeTile}");

                PostHelpers.CreateOpenedLetter(
                    system.EntityManager,
                    freeTile,
                    (int)applianceId,
                    priceMultiplier
                );
            }
        }
    }
}
