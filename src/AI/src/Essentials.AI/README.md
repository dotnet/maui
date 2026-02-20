## Generating Files

To generate the API definitions files:

```
dotnet build src/AI/src/Essentials.AI/Essentials.AI.csproj -f net10.0-ios26.0

sharpie bind \
  --output=src/AI/src/Essentials.AI/Platform/MaciOS \
  --namespace=Microsoft.Maui.Essentials.AI \
  --sdk=iphoneos26.1 \
  --scope=. \
  src/AI/src/Essentials.AI/Users/matthew/Documents/GitHub/maui/artifacts/obj/Essentials.AI/Debug/net10.0-ios26.0/xcode/EssentialsAI-485fe/archives/EssentialsAIiOS.xcarchive/Products/Library/Frameworks/EssentialsAI.framework/Headers/EssentialsAI-Swift.h
```