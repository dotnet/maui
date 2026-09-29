# Wordzzle MAUI sample

This in-tree sample ports Wordzzle's daily word-guessing loop to .NET MAUI. It includes four-to-seven-letter English puzzles, a tappable keyboard, duplicate-letter-aware scoring, local progress persistence, and basic game statistics.

Build from the repository root with the platform target appropriate for your machine:

```bash
dotnet build src/Controls/samples/Wordzzle/Wordzzle.csproj --framework net10.0-android
dotnet build src/Controls/samples/Wordzzle/Wordzzle.csproj --framework net10.0-ios
dotnet build src/Controls/samples/Wordzzle/Wordzzle.csproj --framework net10.0-maccatalyst
```

The sample uses the MAUI source and target-framework settings in this checkout. Its small offline word lists replace the original cloud services; Firebase-backed battles, leaderboards, account sync, ads, notifications, and the other language dictionaries are not included.
