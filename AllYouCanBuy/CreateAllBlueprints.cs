using System.Collections.Generic;
using System.Linq;
using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    public class CreateAllBlueprints : StartOfNightSystem, IModSystem
    {
        private static readonly HashSet<string> ApplianceNames = new HashSet<string>
        {
            "Smart Grabber",
            "Mixer"
        };

        private List<int> _applianceIds = new List<int>();
        private EntityQuery _dayQuery;

        protected override void Initialise()
        {
            base.Initialise();

            _applianceIds = GameData.Main.Get<Appliance>()
                .Where(a => ApplianceNames.Contains(a.Name))
                .Select(a => a.ID)
                .ToList();
        }

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

            foreach (var applianceId in _applianceIds)
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
            var tiles = new Queue<Vector3>();

            for (var z = Mathf.RoundToInt(Bounds.min.z); z <= Mathf.RoundToInt(Bounds.max.z); z++)
            {
                for (var x = Mathf.RoundToInt(Bounds.min.x); x <= Mathf.RoundToInt(Bounds.max.x); x++)
                {
                    var candidate = new Vector3(x, 0f, z);

                    if (TileManager.IsSuitableEmptyTile(candidate))
                    {
                        tiles.Enqueue(candidate);
                    }
                }
            }

            return tiles;
        }
    }
}
