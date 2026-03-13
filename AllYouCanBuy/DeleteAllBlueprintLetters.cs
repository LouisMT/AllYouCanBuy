using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace AllYouCanBuy
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    [UpdateAfter(typeof(HandleNewShop))]
    public class DeleteAllBlueprintLetters : NightSystem, IModSystem
    {
        private EntityQuery _blueprintLetterQuery;
        private EntityQuery _dayQuery;

        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            _dayQuery = GetEntityQuery(ComponentType.ReadOnly<SDay>());
        }

        protected override void Initialise()
        {
            base.Initialise();
            _blueprintLetterQuery = GetEntityQuery(new QueryHelper().All(typeof(CLetterBlueprint)));
        }

        protected override void OnUpdate()
        {
            // Do not delete blueprint letters on the first day and decoration days.
            var day = _dayQuery.GetSingleton<SDay>().Day;
            if (day % 5 == 0) return;

            EntityManager.DestroyEntity(_blueprintLetterQuery);
        }
    }
}