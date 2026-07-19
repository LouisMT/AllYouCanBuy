# Next Page Handoff

## Request

Replace the appliance-level `Cycle Blueprints` rollover with deterministic blueprint pages, a live page fraction, and matching right-facing circled-chevron assets.

The implementation plan is in `PLAN.md`.

## Build Requirement

Always build this project with `dotnet build AllYouCanBuy/AllYouCanBuy.csproj --configuration Release`. `Build.local.props` enables `AutoCopyMod`, so a Release build deploys `AllYouCanBuy.dll` to PlateUp's `Mods/AllYouCanBuy/content` directory. Verify the deployed DLL matches the Release output; a Debug-only build is not sufficient.

## Current State

- Branch: `feature/deterministic-blueprint-pages`.
- Appliance selection uses stable fixed pages and allows a partially filled final page.
- The next-page interaction advances once and wraps at the page level.
- The tile label is `Next Page` and displays the live `CurrentPage/PageCount` fraction after pagination is initialized.
- Normal reroll days restore the localized reroll label, dice, price, duration, and progress glyph.
- The old cycle-arrow mesh and icon have been replaced with matching circled-chevron resources and renamed loaders/generation scripts.
- The progress chevron is matched through the reroll appliance's exact ECS indicator/view identity. Blueprint purchases retain the coin icon.
- The 3D chevron is baked with a 25-degree camera-facing incline and converts the container animation into rotation around its own inclined vertical diameter. Its edge-on flip keeps it visually right-facing.
- Runtime pagination resets when entering HQ, loading a save, or installing a new daily appliance list.

## Verification Performed

- `dotnet build AllYouCanBuy/AllYouCanBuy.csproj --configuration Release` succeeds with 0 warnings and 0 errors.
- The deployed PlateUp DLL has been compared with the Release output and matches.
- `git diff --check` passes.
- Searches find no stale cycle-arrow runtime names or removed `CycleApplianceIds` calls.
- In-game testing confirmed the next-page progress bar uses the chevron while blueprint purchases retain their coin icon.
- In-game testing confirmed the inclined 3D chevron rotates around its own axis and remains visually right-facing.
- In-game testing confirmed loaded shops can page successfully.

No multiplayer verification has been performed.

## Accepted Load Behavior

Pagination state is intentionally runtime-only. When loading a save that already contains shop blueprints, `CreateAllBlueprints` does not run, so the page count remains uninitialized and the title initially shows only `Next Page`. The first page interaction calculates pagination and the fraction then appears. This behavior has been accepted for now.

## Remaining Risks

- Host/client pagination state and page fractions still need multiplayer verification, including late joining.
- The explicit `SimulationSystemGroup` ordering on `HandleNextBlueprintPage` should be checked in a fresh `Player.log` to confirm Unity no longer reports ignored ordering attributes.
- A shop-layout capacity change during the same day recalculates page boundaries.
