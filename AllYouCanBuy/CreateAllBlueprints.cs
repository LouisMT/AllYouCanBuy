using System.Collections.Generic;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    public class CreateAllBlueprints : StartOfNightSystem, IModSystem
    {
        private readonly ApplianceHelper _applianceHelper = new ApplianceHelper();

        private EntityQuery _dayQuery;

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();

            _dayQuery = GetEntityQuery(ComponentType.ReadOnly<SDay>());
        }

        protected override void OnUpdate()
        {
            if (HasSingleton<SIsRestartedDay>() || _dayQuery.GetSingleton<SDay>().Day <= 0)
            {
                return;
            }

            var freeTiles = FindFreeTiles();
            Debug.Log($"Found {freeTiles.Count} free tiles");

            foreach (var applianceId in _applianceHelper.GetApplianceIds())
            {
                if (freeTiles.TryDequeue(out var freeTile))
                {
                    Debug.Log($"Spawning {applianceId} at {freeTile}");
                    PostHelpers.CreateOpenedLetter(EntityManager, freeTile, applianceId);
                }
                else
                {
                    Debug.LogWarning("No free tile found");
                }
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
