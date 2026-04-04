using Unity.Entities;

namespace AllYouCanBuy.Components
{
    public struct SCurrentApplianceIndex: IComponentData
    {
        public int Value { get; set; }
    }
}
