using AllYouCanBuy.Components;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;

namespace AllYouCanBuy.Systems
{
    // The mod keeps its working state in plain static fields (see ApplianceHelper), never as ECS
    // components, so new saves contain no mod-defined type and keep loading after the mod is
    // uninstalled.
    //
    // Saves written by earlier versions of the mod still contain the mod's old entities
    // (SDailyAppliances + the SDailyApplianceId buffer, and SCurrentApplianceIndex). Those component
    // types are kept defined only so such saves still deserialize; the runtime no longer creates or
    // reads them. After loading we destroy any of those entities so the next save no longer contains
    // a mod-defined type.
    //
    // GenericSystemBase implements IPersist, so overriding AfterLoading is enough to be called back;
    // the system does no per-frame work.
    public class ClearPersistedModState : GenericSystemBase, IModSystem
    {
        public override void AfterLoading(SaveSystemType systemType)
        {
            base.AfterLoading(systemType);
            ApplianceHelper.Reset();
#pragma warning disable CS0618 // Type or member is obsolete
            EntityManager.DestroyEntity(GetEntityQuery(typeof(SDailyAppliances)));
            EntityManager.DestroyEntity(GetEntityQuery(typeof(SCurrentApplianceIndex)));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        protected override void OnUpdate() { }
    }
}
