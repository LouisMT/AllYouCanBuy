using System.Linq;
using Kitchen;
using KitchenData;
using KitchenMods;
using UnityEngine;

namespace AllYouCanBuy
{
    public class LogAppliances : GenericSystemBase, IModSystem
    {
        private bool _done;

        protected override void OnUpdate()
        {
            if (_done)
            {
                return;
            }

            var appliances = GameData.Main.Get<Appliance>();

            foreach (var appliance in appliances.Where(a => !string.IsNullOrWhiteSpace(a.Name)))
            {
                Debug.Log($"Name: {appliance.Name}, ID: {appliance.ID}");
            }

            _done = true;
        }
    }
}