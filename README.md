# All You Can Buy

A [PlateUp!](https://store.steampowered.com/app/1599600/PlateUp/) mod that replaces the randomized shop with a fixed one, so there's no surprises in what you can buy.

## Building

1. Install the [.NET SDK](https://dotnet.microsoft.com/en-us/download).
1. Create a copy of `Build.props` named `Build.local.props` and set the value of `PlateUpGamePath`. Both Unix (`/`) and Windows (`\`) paths work.
2. Build using `dotnet build -c Release`. The mod will be immediately installed as a PlateUp! mod.
