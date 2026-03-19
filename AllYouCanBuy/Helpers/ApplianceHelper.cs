using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Constants;


namespace AllYouCanBuy.Helpers
{
    public class ApplianceHelper
    {
        public static ApplianceHelper Main = new ApplianceHelper();

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

        // Appliance IDs needed for the specific dishes.
        private int[] _dailyApplianceIds = { };

        private int _currentApplianceIndex;

        public IEnumerable<int> CycleApplianceIds()
        {
            var allApplianceIds = BaseApplianceIds.Concat(_dailyApplianceIds).Distinct().ToArray();

            for (var i = 0; i < allApplianceIds.Length; i++)
            {
                if (_currentApplianceIndex >= allApplianceIds.Length)
                {
                    _currentApplianceIndex = 0;
                }

                yield return allApplianceIds[_currentApplianceIndex++];
            }
        }

        public void SetDailyApplianceIds(IEnumerable<int> ids) => _dailyApplianceIds = ids.ToArray();
    }
}