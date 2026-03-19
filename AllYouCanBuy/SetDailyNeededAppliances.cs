using System.Collections.Generic;
using System.Linq;
using AllYouCanBuy.Constants;
using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace AllYouCanBuy
{
    [UpdateInGroup(typeof(EndOfDayProgressionGroup))]
    [UpdateBefore(typeof(CreateAllBlueprints))]
    [UpdateAfter(typeof(HandleNewDish))]
    public class SetDailyNeededAppliances : ShopSystem, IModSystem
    {
        // Enabling appliances are always starters, we want the base version in the shop.
        // (For example, for Steak a Starter Hob is given as enabling appliance).
        private static readonly Dictionary<int, int> StarterToBaseApplianceIds = new Dictionary<int, int>
        {
            { ApplianceId.StarterSink, ApplianceId.Sink },
            { ApplianceId.StarterBin, ApplianceId.Bin },
            { ApplianceId.StarterHob, ApplianceId.Hob },
            { ApplianceId.StarterPlates, ApplianceId.Plates }
        };
        
        private EntityQuery _menuItemsQuery;
        private EntityQuery _possibleExtrasQuery;

        protected override void Initialise()
        {
            base.Initialise();
            _menuItemsQuery = GetEntityQuery((ComponentType)typeof(CMenuItem));
            _possibleExtrasQuery = GetEntityQuery((ComponentType)typeof(CPossibleExtra));
        }

        protected override void OnShopUpdate()
        {
            using var menuItems = _menuItemsQuery.ToComponentDataArray<CMenuItem>(Allocator.Temp);
            var menuItemIds = menuItems.Select(m => m.Item).ToList();
            using var possibleExtras = _possibleExtrasQuery.ToComponentDataArray<CPossibleExtra>(Allocator.Temp);

            ApplianceHelper.Main.SetDailyApplianceIds(
                menuItems
                    .Select(m => GameData.Main.Get<Dish>(m.SourceDish))
                    .SelectMany(d => d.RequiredProcesses
                        .Select(p => p.BasicEnablingAppliance.ID)
                        .Concat(
                            d.MinimumIngredients
                                .Select(i => i.ID)
                                .SelectApplianceIdsFromItems()
                        )
                    )
                    .Concat(
                        possibleExtras
                            .Where(e => menuItemIds.Contains(e.MenuItem))
                            .Select(e => e.Ingredient)
                            .SelectApplianceIdsFromItems()
                    )
                    .Distinct()
                    // Map starters to base (e.g. Starter Hob to Hob)
                    // ReSharper disable once CanSimplifyDictionaryTryGetValueWithGetValueOrDefault
                    //                        ^ (does not work in Unity)
                    .Select(id => StarterToBaseApplianceIds.TryGetValue(id, out var mappedId)
                        ? mappedId
                        : id
                    )
                    .SelectMany(id => id switch
                    {
                        // If Plates are present, add DishRack next to Plates in the shop.
                        ApplianceId.Plates => new[] { ApplianceId.Plates, ApplianceId.DishRack },
                        _ => new[] { id }
                    })
            );
        }
    }

    internal static class ApplianceIdSelectors
    {
        internal static IEnumerable<int> SelectApplianceIdsFromItems(this IEnumerable<int> itemIds) =>
            itemIds.Select(id => GameData.Main.Get<Item>(id).DedicatedProvider.ID);
    }
}