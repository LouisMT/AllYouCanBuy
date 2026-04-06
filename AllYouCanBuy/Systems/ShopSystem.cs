using Kitchen;
using Unity.Entities;

namespace AllYouCanBuy.Systems
{
    /**
     * Runs when:
     * <list type="bullet">
     *   <item>
     *     <description>it's night;</description>
     *   </item>
     *   <item>
     *     <description>it's not the first day;</description>
     *   </item>
     *   <item>
     *     <description>there's no pop-ups;</description>
     *   </item>
     *   <item>
     *     <description>and there are no appliance blueprints.</description>
     *   </item>z
     * </list>
     */
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    public abstract class ShopSystem : NightSystem
    {
        private EntityQuery _popupsQuery;
        private EntityQuery _applianceBlueprintsQuery;
        private EntityQuery _dayQuery;

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            _dayQuery = GetEntityQuery(ComponentType.ReadOnly<SDay>());
        }

        protected override void Initialise()
        {
            base.Initialise();
            _popupsQuery = GetEntityQuery((EntityQueryDesc)new QueryHelper().Any(
                (ComponentType)typeof(CPopup))
            );
            _applianceBlueprintsQuery = GetEntityQuery((EntityQueryDesc)new QueryHelper().Any(
                (ComponentType)typeof(CApplianceBlueprint))
            );
        }

        protected sealed override void OnUpdate()
        {
            if (!_popupsQuery.IsEmpty || !_applianceBlueprintsQuery.IsEmpty) return;
            if (_dayQuery.GetSingleton<SDay>().Day == 0) return;
            OnShopUpdate();
        }

        protected abstract void OnShopUpdate();
    }
}