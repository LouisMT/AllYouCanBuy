using AllYouCanBuy.Helpers;
using Kitchen;
using KitchenMods;

namespace AllYouCanBuy.Systems
{
    // ApplianceHelper keeps its state in static fields (see ClearPersistedModState for why), and
    // statics survive the scene transitions that used to wipe it. ClearPersistedModState resets it
    // when a save is loaded, but starting a fresh run goes through no load path at all, so without
    // this a new run would pick up the previous run's appliance list and page.
    //
    // Every run is started from HQ, and being in HQ means no run is active, so entering the franchise
    // scene is a safe point to reset. Only the transition into it is acted on, so the state is not
    // rebuilt every frame while sitting in HQ.
    public class ResetModStateInHq : GenericSystemBase, IModSystem
    {
        private SceneType _sceneType = SceneType.Null;

        protected override void OnUpdate()
        {
            var sceneType = GetOrDefault<SCurrentScene>().Type;
            if (sceneType == _sceneType) return;

            _sceneType = sceneType;
            if (sceneType != SceneType.Franchise) return;

            Logger.Info("Entered HQ, resetting appliance state");
            ApplianceHelper.Reset();
        }
    }
}
