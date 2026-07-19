using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Constants;

namespace AllYouCanBuy.Helpers
{
    public static class ApplianceHelper
    {
        private static readonly ApplianceId[] BaseApplianceIds =
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

        private static List<ApplianceId> _dailyApplianceIds = new List<ApplianceId>();
        private static int _currentApplianceIndex;

        public static ApplianceId[] CycleApplianceIds(int count)
        {
            var allApplianceIds = BaseApplianceIds
                .Concat(_dailyApplianceIds)
                .Distinct()
                .ToArray();

            var result = new ApplianceId[count];
            Logger.Info($"Cycling {count} appliance IDs, starting at {_currentApplianceIndex} ({allApplianceIds.Length} unique appliances available)");

            for (var i = 0; i < count; i++)
            {
                if (_currentApplianceIndex >= allApplianceIds.Length)
                {
                    _currentApplianceIndex = 0;
                }

                result[i] = allApplianceIds[_currentApplianceIndex++];
            }

            return result;
        }

        public static void SetDailyApplianceIds(IEnumerable<ApplianceId> dailyApplianceIds)
        {
            _dailyApplianceIds = dailyApplianceIds.ToList();
        }

        public static void Reset()
        {
            _dailyApplianceIds = new List<ApplianceId>();
            _currentApplianceIndex = 0;
        }
    }
}
