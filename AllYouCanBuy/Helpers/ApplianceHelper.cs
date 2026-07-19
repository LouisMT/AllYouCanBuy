using System;
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

        private static ApplianceId[] _applianceIds = BaseApplianceIds;
        private static int _currentPageIndex;
        private static int _pageCount;

        public static int CurrentPage => _pageCount == 0 ? 0 : _currentPageIndex + 1;
        public static int PageCount => _pageCount;

        public static ApplianceId[] GetCurrentAppliancePage(int pageCapacity)
        {
            UpdatePagination(pageCapacity);
            if (_pageCount == 0)
            {
                return Array.Empty<ApplianceId>();
            }

            var startIndex = _currentPageIndex * pageCapacity;
            var applianceCount = Math.Min(pageCapacity, _applianceIds.Length - startIndex);
            var result = new ApplianceId[applianceCount];
            Array.Copy(_applianceIds, startIndex, result, 0, applianceCount);

            Logger.Info(
                $"Loading appliance page {CurrentPage}/{PageCount} with {applianceCount} of {_applianceIds.Length} appliances"
            );
            return result;
        }

        public static void AdvancePage(int pageCapacity)
        {
            UpdatePagination(pageCapacity);
            if (_pageCount == 0)
            {
                return;
            }

            _currentPageIndex = (_currentPageIndex + 1) % _pageCount;
        }

        public static void SetDailyApplianceIds(IEnumerable<ApplianceId> dailyApplianceIds)
        {
            _applianceIds = BaseApplianceIds
                .Concat(dailyApplianceIds)
                .Distinct()
                .ToArray();
            _currentPageIndex = 0;
            _pageCount = 0;
        }

        public static void Reset()
        {
            _applianceIds = BaseApplianceIds;
            _currentPageIndex = 0;
            _pageCount = 0;
        }

        private static void UpdatePagination(int pageCapacity)
        {
            if (pageCapacity <= 0 || _applianceIds.Length == 0)
            {
                _currentPageIndex = 0;
                _pageCount = 0;
                return;
            }

            _pageCount = (_applianceIds.Length + pageCapacity - 1) / pageCapacity;
            if (_currentPageIndex >= _pageCount)
            {
                _currentPageIndex = 0;
            }
        }
    }
}
