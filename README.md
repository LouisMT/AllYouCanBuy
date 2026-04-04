# All You Can Buy

A [PlateUp!](https://store.steampowered.com/app/1599600/PlateUp/) mod that replaces the randomized shop with a fixed one, so there's no surprises in what you can buy.

## Building

1. Install the [.NET SDK](https://dotnet.microsoft.com/en-us/download).
1. Create a copy of `Build.props` named `Build.local.props` and:
   * Set the value of `PlateUpGamePath`. Both Unix (`/`) and Windows (`\`) paths work.
   * Set the value of `AutoCopyMod` to `true` to automatically copy the mod to the game folder on Release builds.
2. Build using `dotnet build -c Release`.

## CI

The project uses CI to validate PRs and upload the mod to the workshop when a release is created.

When the game gets updated, the `game-version.json` file needs to be updated to invalidate the cache.
