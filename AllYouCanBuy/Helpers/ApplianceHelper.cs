using System;
using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Components;
using AllYouCanBuy.Constants;
using Kitchen;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    public static class ApplianceHelper
    {
        private static readonly int[] BaseApplianceIds =
        {
            ApplianceId.Counter,
            ApplianceId.PrepStation,
            ApplianceId.Mixer,
            ApplianceId.TrayStand,
            ApplianceId.DiningTable,
            ApplianceId.ResearchDesk,
            ApplianceId.BlueprintCabinet,
            ApplianceId.BookingDesk,
            ApplianceId.ClipboardStand,
            ApplianceId.Mop,
            ApplianceId.FloorBuffer,
            ApplianceId.KitchenFloorProtector,
            ApplianceId.Buffet,
            ApplianceId.Conveyor,
            ApplianceId.Combiner,
            ApplianceId.Portioner,
            ApplianceId.Dumbwaiter,
            ApplianceId.GasLimiter,
            ApplianceId.GasOverrider,
            ApplianceId.CoffeeTable,
            ApplianceId.HostingStand,
            ApplianceId.DisplayStand,
            ApplianceId.SpecialsTerminal,
            ApplianceId.OrderingTerminal,
            ApplianceId.Bin,
            ApplianceId.FireExtinguisher,
            ApplianceId.Napkins,
            ApplianceId.SharpCutlery,
            ApplianceId.LeftoverBags,
            ApplianceId.SpecialsMenu,
            ApplianceId.Breadsticks,
            ApplianceId.CandleBox,
            ApplianceId.FlowerPot,
            ApplianceId.Supplies,
            ApplianceId.ScrubbingBrush,
            ApplianceId.SharpKnife,
            ApplianceId.RollingPin,
            ApplianceId.Trainers,
            ApplianceId.Wellies,
            ApplianceId.WorkBoots
        };

        public static int[] CycleApplianceIds(GenericSystemBase system, int count)
        {
            var dailyAppliancesEntity = system.GetSingletonEntity<SDailyAppliances>();
            var dailyApplianceIdBuffer = system.EntityManager.GetBuffer<SDailyApplianceId>(dailyAppliancesEntity)
                .ToNativeArray(Allocator.Temp);

            var allApplianceIds = BaseApplianceIds
                .Concat(dailyApplianceIdBuffer
                    .Select(s => s.Value)
                )
                .Distinct()
                .ToArray();

            var result = new int[count];
            var currentApplianceIndex = GetCurrentApplianceIndex(system);
            Debug.Log($"Cycling {count} appliance IDs, starting at {currentApplianceIndex} ({allApplianceIds.Length} unique appliances available)");

            for (var i = 0; i < count; i++)
            {
                if (currentApplianceIndex >= allApplianceIds.Length)
                {
                    currentApplianceIndex = 0;
                }

                result[i] = allApplianceIds[currentApplianceIndex++];
            }

            SetCurrentApplianceIndex(system, currentApplianceIndex);

            return result;
        }

        public static void SetDailyApplianceIds(GenericSystemBase system, IEnumerable<int> dailyApplianceIds)
        {
            if (system is null)
            {
                throw new InvalidOperationException("InitialiseApplianceHelper must be called first");
            }

            var dailyApplianceIdBuffer = GetDailyAppliances(system);

            dailyApplianceIdBuffer.Clear();

            foreach (var dailyApplianceId in dailyApplianceIds)
            {
                dailyApplianceIdBuffer.Add(new SDailyApplianceId
                {
                    Value = dailyApplianceId
                });
            }
        }

        private static int GetCurrentApplianceIndex(GenericSystemBase system)
        {
            if (!system.HasSingleton<SCurrentApplianceIndex>())
            {
                var entity = system.EntityManager.CreateEntity();
                system.EntityManager.AddComponentData(entity, new SCurrentApplianceIndex
                {
                    Value = 0
                });
            }

            return system.GetSingleton<SCurrentApplianceIndex>().Value;
        }

        private static void SetCurrentApplianceIndex(GenericSystemBase system, int currentApplianceIndex)
        {
            system.SetSingleton(new SCurrentApplianceIndex
            {
                Value = currentApplianceIndex
            });
        }

        private static DynamicBuffer<SDailyApplianceId> GetDailyAppliances(GenericSystemBase system)
        {
            if (!system.HasSingleton<SDailyAppliances>())
            {
                var entity = system.EntityManager.CreateEntity();
                system.EntityManager.AddComponentData(entity, new SDailyAppliances());
                system.EntityManager.AddBuffer<SDailyApplianceId>(entity);
            }

            var dailyAppliancesEntity = system.GetSingletonEntity<SDailyAppliances>();
            return system.EntityManager.GetBuffer<SDailyApplianceId>(dailyAppliancesEntity);
        }
    }
}
