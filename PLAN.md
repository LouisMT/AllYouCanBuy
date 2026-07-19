# Next Page Blueprint Plan

## Goal

Replace the current appliance-level "Cycle Blueprints" rollover with deterministic blueprint pages. For a given day and shop layout, each page must always contain the same appliances in the same order. A page contains at most the number of available blueprint tiles, so the final page is allowed to be partially filled. The button advances from the final page back to page 1, but appliances must never wrap from the start of the list into the final page.

## Implementation

### 1. Replace appliance rollover with stable pagination

- Refactor `AllYouCanBuy/Helpers/ApplianceHelper.cs` so it builds one stable, deduplicated appliance list for the day and tracks a zero-based current page rather than a continuously advancing appliance index.
- Replace `CycleApplianceIds(int count)` with page-oriented operations that:
  - Calculate `totalPages` as the ceiling of `applianceCount / pageCapacity`.
  - Return only the fixed slice for the current page: `pageIndex * pageCapacity` through the end of that page.
  - Do not fill unused spaces on the final page with appliances from page 1.
  - Expose the one-based current page and total page count for the UI.
  - Advance to the next page after a reroll request, wrapping from the final page to page 1 at the page level only.
- Update `AllYouCanBuy/Helpers/BlueprintPageHelper.cs` to spawn the current page's appliance slice and to use only the corresponding number of free tiles. Keep tile ordering stable so returning to a page reproduces the same appliance placement.
- Update `AllYouCanBuy/Systems/CreateAllBlueprints.cs` to create page 1 without implicitly advancing it, and update `AllYouCanBuy/Systems/HandleNextBlueprintPage.cs` to advance exactly once before replacing the displayed blueprints.
- Reset the page to page 1 when `SetDailyApplianceIds` installs the next day's list and when `ResetModStateInHq` starts a new run. Keep the state runtime-only, preserving the current save compatibility approach and obsolete ECS cleanup types.
- Guard invalid or empty page capacities so pagination cannot divide by zero or index an empty page.
- Confirm the synchronized reroll request remains the only action that changes pages, so hosts and multiplayer clients display the same page.

### 2. Change the button text and show page position

- Change `NextBlueprintPageView.Label` in `AllYouCanBuy/Views/NextBlueprintPageView.cs` from `Cycle Blueprints` to `Next Page`.
- Render both the action and current page status on the tile, for example `Next Page\n1/3`, using the current and total values exposed by the pagination state.
- Refresh the title after initial page creation, after every page change, and after a new day resets pagination. Do not leave the page fraction captured as a one-time value during view initialization.
- Preserve restoration of the original localized reroll label when the tile is not the free next-page action, including normal reroll days where `Day % 5 == 0`.
- Update cycle-oriented runtime names touched by this work (`IsCycle`, progress view/manager names, log messages, and generated GameObject names) to next-page terminology so the code and diagnostics describe the new behavior.

### 3. Replace the flat 3D floor model with a circled chevron

- Update or replace `art/create_cycle_arrows.py` to generate a single right-facing circled-chevron symbol based on the reference: <https://img.icons8.com/m_sharp/1200/circled-chevron-right.jpg>.
- Model a clean circular face with a clearly readable right chevron inset/cutout, using the existing shallow extrusion and softened bevel style.
- Keep the symbol horizontal in Unity's XZ plane, flat on the floor like the current arrows, and preserve compatible scale, floor clearance, and inherited tile material behavior in `NextBlueprintPageView.ReplaceDice`.
- Regenerate the Blender source and use `art/export_cycle_arrows_mesh.py` (renamed for the new asset) to export the embedded mesh resource.
- Rename the `.blend`, `.mesh`, mesh loader, resource name, object names, and `AllYouCanBuy.csproj` embedded-resource entry away from `CycleBlueprintArrows` to circled-chevron/next-page terminology. Remove the superseded cycle-arrow resources after all references have moved.

### 4. Replace the 2D progress icon with the same circled chevron

- Update or replace `art/create_cycle_arrows_icon.py` to draw the same proportions and right-facing circled-chevron silhouette as the 3D model, with a transparent exterior and high-contrast monochrome treatment suitable for the existing progress UI.
- Regenerate the raw RGBA embedded asset at an antialiased resolution and retain the correct bottom-up orientation expected by Unity.
- Rename `CycleBlueprintArrowsIcon`, its `.rgba` resource, sprite/texture names, progress-view references, and the `AllYouCanBuy.csproj` embedded-resource entry to next-page circled-chevron terminology.
- Check the sprite's local scale, centering, sorting layer, and sorting order in the progress view; adjust only as needed so it occupies the same visual footprint as the current arrows without clipping.

## Verification

- Build with `dotnet build AllYouCanBuy/AllYouCanBuy.csproj --configuration Release` using the local PlateUp references, then verify the deployed DLL matches the Release output.
- On a day with multiple pages, record every page, wrap back to page 1, and verify each page repeats the same appliances in the same tile positions.
- Verify the displayed title starts at `Next Page` with `1/N`, increments once per interaction, and returns to `1/N` when the final page advances to the first.
- Verify a non-even appliance count produces a partially filled final page with no duplicates from page 1 and no stale blueprints left on unused tiles.
- Verify a new day starts on page 1 and recomputes the total from that day's appliance list and available tile count.
- Verify normal reroll days (`Day % 5 == 0`) retain the original reroll label, dice, price, and behavior.
- Verify host and multiplayer clients see the same appliances, page fraction, 3D tile symbol, and 2D progress icon after every page change.
- Inspect the 3D circled chevron in game to confirm it lies flat, faces right from the player's viewing direction, remains above the floor without visible floating or z-fighting, and uses the expected material.
- Inspect the 2D circled chevron at gameplay scale to confirm its direction, alpha edges, centering, contrast, and sorting are correct.
- Search the project for stale `Cycle Blueprints` and `CycleBlueprintArrows` references, allowing only intentional migration/save-compatibility names if any remain.
