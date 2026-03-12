using System.Collections.Generic;
using System.Linq;
using KitchenData;

namespace AllYouCanBuy.Helpers
{
    public class ApplianceHelper
    {
        private static readonly HashSet<string> ApplianceNames = new HashSet<string>
        {
            "Smart Grabber",
            "Heated Mixer",
            "Conveyor Mixer",
            "Rapid Mixer",
            "Composter Bin",
            "Kitchen Floor Protector",
            "Conveyor",
            "Combiner",
            "Portioner",
            "Workstation",
            "Danger Hob",
            "Safety Hob",
            "Microwave",
            "Auto Plater",
            "Plates",
            "Frozen Prep Station",
            "Dish Washer",
            "Buffet",
            "Grabber",
            "Grabber - Rotating"
        };

        private readonly List<int> _applianceIds;

        private int _currentApplianceIndex;

        public ApplianceHelper()
        {
            _applianceIds = GameData.Main.Get<Appliance>()
                .Where(a => ApplianceNames.Contains(a.Name))
                .Select(a => a.ID)
                .ToList();
        }

        public IEnumerable<int> GetApplianceIds()
        {
            for (var i = 0; i < _applianceIds.Count; i++)
            {
                if (_currentApplianceIndex >= _applianceIds.Count)
                {
                    _currentApplianceIndex = 0;
                }

                yield return _applianceIds[_currentApplianceIndex++];
            }
        }
    }
}