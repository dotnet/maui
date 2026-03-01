# Swift Templates for iOS Widget Extension

Replace placeholders: `{GroupId}`, `{UrlScheme}`, `{UrlHost}`, `{WidgetKind}`, `{ExtensionName}`

## Settings.swift

Mirrors `WidgetConstants.cs` — must match exactly.

```swift
enum Settings {
    static let groupId = "{GroupId}"
    static let fromAppFile = "widget_data_fromapp.json"
    static let fromWidgetFile = "widget_data_fromwidget.json"
    static let urlScheme = "{UrlScheme}"
    static let urlHost = "{UrlHost}"
    static let widgetKind = "{WidgetKind}"
}
```

## WidgetData.swift

Mirrors `WidgetData.cs` — property names must match `[JsonPropertyName]` values.

```swift
import Foundation

struct WidgetData: Codable {
    var version: Int = 1
    var title: String = ""
    var message: String = ""
    var counter: Int = 0
    var updatedAt: String = ""
    var extras: [String: String] = [:]
}
```

## SharedStorage.swift

JSON file I/O via App Group container. **Do NOT use UserDefaults for cross-process data.**

```swift
import Foundation

struct SharedStorage {
    static func read(filename: String) -> WidgetData? {
        guard let url = containerURL(for: filename),
              let data = try? Data(contentsOf: url) else { return nil }
        return try? JSONDecoder().decode(WidgetData.self, from: data)
    }

    static func write(_ widgetData: WidgetData, filename: String) {
        guard let url = containerURL(for: filename),
              let data = try? JSONEncoder().encode(widgetData) else { return }
        try? data.write(to: url)
    }

    private static func containerURL(for filename: String) -> URL? {
        FileManager.default
            .containerURL(forSecurityApplicationGroupIdentifier: Settings.groupId)?
            .appendingPathComponent(filename)
    }
}
```

## SimpleEntry.swift

```swift
import WidgetKit

struct SimpleEntry: TimelineEntry {
    let date: Date
    let title: String
    let message: String
    let counter: Int
    let deepLinkURL: URL?
}
```

## Provider.swift

For display-only widgets, use `TimelineProvider`. For interactive widgets with AppIntents, use `AppIntentTimelineProvider`.

### Display-only (TimelineProvider)

```swift
import WidgetKit

struct Provider: TimelineProvider {
    func placeholder(in context: Context) -> SimpleEntry {
        SimpleEntry(date: Date(), title: "My App", message: "Loading...", counter: 0, deepLinkURL: nil)
    }

    func getSnapshot(in context: Context, completion: @escaping (SimpleEntry) -> Void) {
        completion(makeEntry())
    }

    func getTimeline(in context: Context, completion: @escaping (Timeline<SimpleEntry>) -> Void) {
        let entry = makeEntry()
        let next = Calendar.current.date(byAdding: .minute, value: 15, to: Date())!
        completion(Timeline(entries: [entry], policy: .after(next)))
    }

    private func makeEntry() -> SimpleEntry {
        let data = SharedStorage.read(filename: Settings.fromAppFile) ?? WidgetData()
        let url = URL(string: "\(Settings.urlScheme)://\(Settings.urlHost)?counter=\(data.counter)")
        return SimpleEntry(
            date: Date(),
            title: data.title.isEmpty ? "My App" : data.title,
            message: data.message.isEmpty ? "Open app" : data.message,
            counter: data.counter,
            deepLinkURL: url
        )
    }
}
```

### Interactive (AppIntentTimelineProvider)

```swift
import WidgetKit

struct Provider: AppIntentTimelineProvider {
    func placeholder(in context: Context) -> SimpleEntry {
        SimpleEntry(date: Date(), title: "My App", message: "Loading...", counter: 0, deepLinkURL: nil)
    }

    func snapshot(for configuration: ConfigurationAppIntent, in context: Context) async -> SimpleEntry {
        makeEntry(configuration: configuration)
    }

    func timeline(for configuration: ConfigurationAppIntent, in context: Context) async -> Timeline<SimpleEntry> {
        let entry = makeEntry(configuration: configuration)
        let next = Calendar.current.date(byAdding: .minute, value: 15, to: Date())!
        return Timeline(entries: [entry], policy: .after(next))
    }

    private func makeEntry(configuration: ConfigurationAppIntent? = nil) -> SimpleEntry {
        let data = SharedStorage.read(filename: Settings.fromAppFile) ?? WidgetData()
        let url = URL(string: "\(Settings.urlScheme)://\(Settings.urlHost)?counter=\(data.counter)")
        return SimpleEntry(
            date: Date(),
            title: configuration?.widgetTitle ?? data.title,
            message: data.message.isEmpty ? "Open app" : data.message,
            counter: data.counter,
            deepLinkURL: url
        )
    }
}
```

## SimpleWidgetView.swift

```swift
import SwiftUI
import WidgetKit

struct SimpleWidgetView: View {
    var entry: Provider.Entry

    var body: some View {
        VStack(spacing: 6) {
            Text(entry.title)
                .font(.headline)
                .lineLimit(1)

            Text("\(entry.counter)")
                .font(.system(size: 48, weight: .bold))

            Text(entry.message)
                .font(.caption)
                .foregroundColor(.secondary)
                .lineLimit(2)
        }
        .padding()
        .widgetURL(entry.deepLinkURL)
        .containerBackground(.fill.tertiary, for: .widget)
    }
}
```

## SimpleWidget.swift

```swift
import SwiftUI
import WidgetKit

struct SimpleWidget: Widget {
    let kind: String = Settings.widgetKind

    var body: some WidgetConfiguration {
        // For display-only:
        StaticConfiguration(kind: kind, provider: Provider()) { entry in
            SimpleWidgetView(entry: entry)
        }
        // For interactive (AppIntents):
        // AppIntentConfiguration(kind: kind, provider: Provider()) { entry in
        //     SimpleWidgetView(entry: entry)
        // }
        .configurationDisplayName("My Widget")
        .description("Shows data from the app.")
        .supportedFamilies([.systemSmall, .systemMedium])
    }
}
```

## SimpleWidgetBundle.swift

```swift
import WidgetKit
import SwiftUI

@main
struct SimpleWidgetBundle: WidgetBundle {
    var body: some Widget {
        SimpleWidget()
    }
}
```

## AppIntents (Optional — for interactive widgets)

### Intents/ConfigurationAppIntent.swift

```swift
import AppIntents
import WidgetKit

struct ConfigurationAppIntent: WidgetConfigurationIntent {
    static var title: LocalizedStringResource { "Configure Widget" }
    static var description: IntentDescription { "Customize the widget appearance." }

    @Parameter(title: "Widget Title", default: "My App")
    var widgetTitle: String
}
```

### Intents/IncrementCounterIntent.swift

```swift
import AppIntents
import WidgetKit

struct IncrementCounterIntent: AppIntent {
    static var title: LocalizedStringResource { "Increment Counter" }

    func perform() async throws -> some IntentResult {
        var data = SharedStorage.read(filename: Settings.fromWidgetFile) ?? WidgetData()
        data.counter += 1
        data.updatedAt = ISO8601DateFormatter().string(from: Date())
        data.message = "Incremented by widget"
        SharedStorage.write(data, filename: Settings.fromWidgetFile)
        WidgetCenter.shared.reloadTimelines(ofKind: Settings.widgetKind)
        return .result()
    }
}
```

### Using interactive buttons in the view

```swift
// Add to SimpleWidgetView body (requires .systemMedium or larger):
HStack(spacing: 16) {
    Button(intent: DecrementCounterIntent()) {
        Image(systemName: "minus.circle.fill")
            .font(.title2)
    }
    Button(intent: IncrementCounterIntent()) {
        Image(systemName: "plus.circle.fill")
            .font(.title2)
    }
}
```

## Widget Info.plist

Full CFBundle keys required — minimal plists cause AppIntentsSSUTraining failures.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleDisplayName</key>
    <string>My Widget</string>
    <key>CFBundleExecutable</key>
    <string>{ExtensionName}</string>
    <key>CFBundleIdentifier</key>
    <string>com.example.myapp.widgetextension</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>{ExtensionName}</string>
    <key>CFBundlePackageType</key>
    <string>XPC!</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>MinimumOSVersion</key>
    <string>17.0</string>
    <key>NSExtension</key>
    <dict>
        <key>NSExtensionPointIdentifier</key>
        <string>com.apple.widgetkit-extension</string>
    </dict>
</dict>
</plist>
```
