# Swift Templates for watchOS Companion App

Replace placeholders: `{AppBundleId}`, `{WatchBundleId}`, `{WatchAppName}`

## PhoneConnectivityProvider.swift

The core WCSession wrapper for the watchOS side. Published properties drive SwiftUI updates.

**⚠️ CRITICAL:** This must be a singleton (`@MainActor` class with shared instance or `@StateObject`). WCSession.default is process-wide — only one delegate allowed.

```swift
import Foundation
import WatchConnectivity

class PhoneConnectivityProvider: NSObject, ObservableObject, WCSessionDelegate {
    @Published var counter: Int = 0
    @Published var title: String = ""
    @Published var message: String = ""
    @Published var isReachable: Bool = false

    override init() {
        super.init()
        if WCSession.isSupported() {
            let session = WCSession.default
            session.delegate = self
            session.activate()
        }
    }

    // MARK: - Send to phone

    func sendContext() {
        guard WCSession.default.activationState == .activated else { return }
        let ctx: [String: Any] = [
            "counter": counter,
            "title": title,
            "message": "Counter: \(counter)",
            "updatedAt": ISO8601DateFormatter().string(from: Date())
        ]
        do {
            try WCSession.default.updateApplicationContext(ctx)
        } catch {
            print("[Watch] updateApplicationContext error: \(error)")
        }
    }

    func sendMessage() {
        guard WCSession.default.isReachable else { return }
        let msg: [String: Any] = [
            "counter": counter,
            "title": title,
            "message": "Counter: \(counter)",
            "updatedAt": ISO8601DateFormatter().string(from: Date())
        ]
        WCSession.default.sendMessage(msg, replyHandler: nil) { error in
            print("[Watch] sendMessage error: \(error)")
        }
    }

    /// Send via both context (reliable) and message (real-time if reachable)
    func sync() {
        sendContext()
        if WCSession.default.isReachable {
            sendMessage()
        }
    }

    // MARK: - WCSessionDelegate

    func session(_ session: WCSession, activationDidCompleteWith activationState: WCSessionActivationState, error: Error?) {
        DispatchQueue.main.async {
            self.isReachable = session.isReachable
        }
        if let error = error {
            print("[Watch] Activation error: \(error)")
        }
    }

    func session(_ session: WCSession, didReceiveApplicationContext applicationContext: [String: Any]) {
        DispatchQueue.main.async {
            self.applyData(applicationContext)
        }
    }

    func session(_ session: WCSession, didReceiveMessage message: [String: Any]) {
        DispatchQueue.main.async {
            self.applyData(message)
        }
    }

    func sessionReachabilityDidChange(_ session: WCSession) {
        DispatchQueue.main.async {
            self.isReachable = session.isReachable
        }
    }

    // MARK: - Private

    private func applyData(_ dict: [String: Any]) {
        if let c = dict["counter"] as? Int { counter = c }
        if let t = dict["title"] as? String { title = t }
        if let m = dict["message"] as? String { message = m }
    }
}
```

## ContentView.swift

The main watch UI. Uses `@StateObject` to own the connectivity provider.

```swift
import SwiftUI

struct ContentView: View {
    @StateObject private var connectivity = PhoneConnectivityProvider()

    var body: some View {
        VStack(spacing: 12) {
            Text(connectivity.title.isEmpty ? "Watch App" : connectivity.title)
                .font(.headline)
                .lineLimit(1)

            Text("\(connectivity.counter)")
                .font(.system(size: 48, weight: .bold))
                .foregroundColor(.purple)

            HStack(spacing: 20) {
                Button(action: {
                    connectivity.counter -= 1
                    connectivity.sync()
                }) {
                    Image(systemName: "minus.circle.fill")
                        .font(.title2)
                        .foregroundColor(.red)
                }

                Button(action: {
                    connectivity.counter += 1
                    connectivity.sync()
                }) {
                    Image(systemName: "plus.circle.fill")
                        .font(.title2)
                        .foregroundColor(.green)
                }
            }

            Text(connectivity.isReachable ? "Phone reachable ✓" : "Phone not reachable")
                .font(.caption2)
                .foregroundColor(connectivity.isReachable ? .green : .gray)
        }
        .padding()
    }
}

#Preview {
    ContentView()
}
```

## MyWatchAppApp.swift

The `@main` entry point for the watchOS app.

```swift
import SwiftUI

@main
struct MyWatchAppApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}
```

## Watch App Info.plist

Full CFBundle keys required for watchOS app.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleDisplayName</key>
    <string>{WatchAppName}</string>
    <key>CFBundleExecutable</key>
    <string>{WatchAppName}</string>
    <key>CFBundleIdentifier</key>
    <string>{WatchBundleId}</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>{WatchAppName}</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>MinimumOSVersion</key>
    <string>10.0</string>
    <key>WKApplication</key>
    <true/>
    <key>WKCompanionAppBundleIdentifier</key>
    <string>{AppBundleId}</string>
</dict>
</plist>
```

**Key Info.plist entries:**
- `WKApplication: true` — marks this as a watchOS app (not a WatchKit extension)
- `WKCompanionAppBundleIdentifier` — must match the iOS app bundle ID exactly
