using Unity.Entities;

namespace AllYouCanBuy.Components
{
    public struct SDailyApplianceId: IBufferElementData
    {
        public int Value { get; set; }
    }
}
