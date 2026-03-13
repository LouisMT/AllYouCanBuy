using System.Collections.Generic;
using AllYouCanBuy.Constants;

namespace AllYouCanBuy.Helpers
{
    public class ApplianceHelper
    {
        private static readonly int[] ApplianceIds = {
            ApplianceId.SmartGrabber,
            ApplianceId.HeatedMixer,
            ApplianceId.ConveyorMixer,
            ApplianceId.RapidMixer,
            ApplianceId.ComposterBin,
            ApplianceId.KitchenFloorProtector,
            ApplianceId.Conveyor,
            ApplianceId.Combiner,
            ApplianceId.Portioner,
            ApplianceId.Workstation,
            ApplianceId.DangerHob,
            ApplianceId.SafetyHob,
            ApplianceId.Microwave,
            ApplianceId.AutoPlater,
            ApplianceId.Plates,
            ApplianceId.FrozenPrepStation,
            ApplianceId.DishWasher,
            ApplianceId.Buffet,
            ApplianceId.Grabber,
            ApplianceId.GrabberRotating
        };

        private int _currentApplianceIndex;

        public IEnumerable<int> CycleApplianceIds()
        {
            for (var i = 0; i < ApplianceIds.Length; i++)
            {
                if (_currentApplianceIndex >= ApplianceIds.Length)
                {
                    _currentApplianceIndex = 0;
                }

                yield return ApplianceIds[_currentApplianceIndex++];
            }
        }
    }
}