using System;
using AllYouCanBuy.Constants;
using Unity.Entities;

namespace AllYouCanBuy.Components
{
    [Obsolete("Do not use or change. Needs to stay so Unity can deserialize old saves that contain this type.")]
    public struct SDailyApplianceId : IBufferElementData
    {
        public ApplianceId Value { get; set; }
    }
}
