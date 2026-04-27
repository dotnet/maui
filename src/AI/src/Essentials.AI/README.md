# Microsoft.Maui.Essentials.AI

## Overview

`Microsoft.Maui.Essentials.AI` is an **experimental** .NET MAUI library that provides cross-platform APIs for working with on-device AI and local models. It exposes platform AI capabilities (such as Apple Intelligence on iOS/macOS) through a unified `Microsoft.Extensions.AI`-compatible interface, so you can use the same `IChatClient` and embedding abstractions across all platforms.

### Supported platforms

| Platform | Support |
|----------|---------|
| iOS | ✅ (Apple Intelligence / Core ML) |
| macOS (Mac Catalyst) | ✅ (Apple Intelligence / Core ML) |
| Android | ✅ |
| Windows | ✅ |

### Add to your project

```xml
<PackageReference Include="Microsoft.Maui.Essentials.AI" Version="*-*" />
```

> **Note:** This package is always released as a preview (e.g. `x.y.z-preview.n`) even when the rest of .NET MAUI ships a stable version.

### Sample app

See `src/AI/samples/Essentials.AI.Sample/` for a complete example — an AI-powered trip-planner using a multi-agent workflow with streaming responses.

---

## Generating Files

To generate the API definitions files:

```bash
dotnet build src/AI/src/Essentials.AI/Essentials.AI.csproj -f net10.0-ios26.0

# Replace <repo-root> with the absolute path to your local maui repository root
sharpie bind \
  --output=src/AI/src/Essentials.AI/Platform/MaciOS \
  --namespace=Microsoft.Maui.Essentials.AI \
  --sdk=iphoneos26.1 \
  --scope=. \
  <repo-root>/artifacts/obj/Essentials.AI/Debug/net10.0-ios26.0/xcode/EssentialsAI-485fe/archives/EssentialsAIiOS.xcarchive/Products/Library/Frameworks/EssentialsAI.framework/Headers/EssentialsAI-Swift.h
```