using AllYouCanBuy.Constants;
using Unity.Entities;

namespace AllYouCanBuy.Components
{
    public struct SDailyApplianceId : IBufferElementData
    {
        public ApplianceId Value { get; set; }
    }
}
