using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Constants;
using KitchenData;

namespace AllYouCanBuy.Extensions
{
    public static class ApplianceIdExtension
    {
        private static readonly Dictionary<ApplianceId, string> ApplianceNameById = GameData.Main
            .Get<Appliance>()
            .ToDictionary(a => (ApplianceId)a.ID, a => a.Name);

        public static string ToName(this ApplianceId applianceId) => ApplianceNameById[applianceId];
    }
}
