import WidgetKit
import SwiftUI

// MARK: - Timeline Entry

struct MauiWidgetEntry: TimelineEntry {
    let date: Date
    let title: String
    let message: String
    let emoji: String
}

// MARK: - Timeline Provider

struct MauiWidgetProvider: TimelineProvider {
    let appGroupId = "group.com.microsoft.maui.sandbox"

    func placeholder(in context: Context) -> MauiWidgetEntry {
        MauiWidgetEntry(date: Date(), title: "MAUI Widget", message: "Loading...", emoji: "🔄")
    }

    func getSnapshot(in context: Context, completion: @escaping (MauiWidgetEntry) -> Void) {
        completion(loadEntry())
    }

    func getTimeline(in context: Context, completion: @escaping (Timeline<MauiWidgetEntry>) -> Void) {
        let entry = loadEntry()
        let nextUpdate = Calendar.current.date(byAdding: .minute, value: 15, to: Date())!
        let timeline = Timeline(entries: [entry], policy: .after(nextUpdate))
        completion(timeline)
    }

    private func loadEntry() -> MauiWidgetEntry {
        // Try App Group defaults
        if let defaults = UserDefaults(suiteName: appGroupId),
           let title = defaults.string(forKey: "widget_title") {
            let message = defaults.string(forKey: "widget_message") ?? "Tap to open"
            let emoji = defaults.string(forKey: "widget_emoji") ?? "🟣"
            return MauiWidgetEntry(date: Date(), title: title, message: message, emoji: emoji)
        }

        // Try shared JSON file (works on simulator without provisioning)
        if let data = sharedFileData(),
           let json = try? JSONSerialization.jsonObject(with: data) as? [String: String] {
            return MauiWidgetEntry(
                date: Date(),
                title: json["title"] ?? "MAUI Sandbox",
                message: json["message"] ?? "Tap to open",
                emoji: json["emoji"] ?? "🟣")
        }
        
        // Default
        return MauiWidgetEntry(
            date: Date(),
            title: "MAUI Sandbox",
            message: "Tap to open app",
            emoji: "🟣")
    }

    private func sharedFileData() -> Data? {
        // The main app writes a JSON file to the shared AppGroup container
        // On simulator, try well-known path as fallback
        let fm = FileManager.default
        if let containerURL = fm.containerURL(forSecurityApplicationGroupIdentifier: appGroupId) {
            let fileURL = containerURL.appendingPathComponent("widget_data.json")
            return try? Data(contentsOf: fileURL)
        }
        return nil
    }
}

// MARK: - Widget View

struct MauiWidgetEntryView: View {
    var entry: MauiWidgetProvider.Entry

    @Environment(\.widgetFamily) var family

    var body: some View {
        switch family {
        case .systemSmall:
            smallView
        default:
            mediumView
        }
    }

    var smallView: some View {
        VStack(spacing: 6) {
            Text(entry.emoji)
                .font(.system(size: 36))
            Text(entry.title)
                .font(.headline)
                .lineLimit(1)
            Text(entry.message)
                .font(.caption)
                .foregroundColor(.secondary)
                .lineLimit(2)
                .multilineTextAlignment(.center)
        }
        .padding()
        .containerBackground(.fill.tertiary, for: .widget)
    }

    var mediumView: some View {
        HStack(spacing: 16) {
            Text(entry.emoji)
                .font(.system(size: 48))
            VStack(alignment: .leading, spacing: 4) {
                Text(entry.title)
                    .font(.headline)
                Text(entry.message)
                    .font(.subheadline)
                    .foregroundColor(.secondary)
                Text(entry.date, style: .time)
                    .font(.caption2)
                    .foregroundColor(.gray)
            }
            Spacer()
        }
        .padding()
        .containerBackground(.fill.tertiary, for: .widget)
    }
}

// MARK: - Widget Configuration

@main
struct MauiSandboxWidget: Widget {
    let kind: String = "MauiSandboxWidget"

    var body: some WidgetConfiguration {
        StaticConfiguration(kind: kind, provider: MauiWidgetProvider()) { entry in
            MauiWidgetEntryView(entry: entry)
        }
        .configurationDisplayName("MAUI Sandbox")
        .description("Shows data shared from the .NET MAUI app.")
        .supportedFamilies([.systemSmall, .systemMedium])
    }
}
