# Microsoft.Maui.Essentials.AI

## Overview

`Microsoft.Maui.Essentials.AI` is an **experimental** .NET MAUI library that exposes platform on-device AI capabilities through a unified `Microsoft.Extensions.AI`-compatible interface. Today it ships built-in `IChatClient` and `IEmbeddingGenerator` implementations on Apple platforms (via Apple Intelligence and Core ML / Natural Language). The package builds for Android and Windows as well, but no built-in on-device providers are wired up for those platforms yet.

### Supported platforms

| Platform | Support |
|----------|---------|
| iOS | ✅ (Apple Intelligence / Core ML) |
| Mac Catalyst | ✅ (Apple Intelligence / Core ML) |
| macOS | ✅ (Apple Intelligence / Core ML) |
| Android | 🚧 Planned (no built-in provider yet) |
| Windows | 🚧 Planned (no built-in provider yet) |

### Add to your project

```xml
<PackageReference Include="Microsoft.Maui.Essentials.AI" Version="10.0.0-preview.1" />
```

> **Note:** This package is always released as a preview (e.g. `x.y.z-preview.n`) even when the rest of .NET MAUI ships a stable version. Replace the version above with the latest `*-preview.*` available on NuGet, or omit `Version` entirely if you use [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management).

### Sample app

See `src/AI/samples/Essentials.AI.Sample/` for a complete example — an AI-powered trip-planner using a multi-agent workflow with streaming responses. Note that this sample demonstrates a **cloud-based** multi-agent pattern and requires Azure OpenAI / OpenAI credentials; it does not exercise the on-device `AppleIntelligenceChatClient` that ships with this package.

---

## Generating Files

To generate the API definitions files:

```bash
dotnet build src/AI/src/Essentials.AI/Essentials.AI.csproj -f net10.0-ios26.0

# Run from the repository root. The 'EssentialsAI-XXXXX' folder name is generated per build —
# locate yours under artifacts/obj/Essentials.AI/Debug/<TFM>/xcode/ before running this command.
sharpie bind \
  --output=src/AI/src/Essentials.AI/Platform/MaciOS \
  --namespace=Microsoft.Maui.Essentials.AI \
  --sdk=iphoneos26.1 \
  --scope=. \
  artifacts/obj/Essentials.AI/Debug/net10.0-ios26.0/xcode/EssentialsAI-XXXXX/archives/EssentialsAIiOS.xcarchive/Products/Library/Frameworks/EssentialsAI.framework/Headers/EssentialsAI-Swift.h
```