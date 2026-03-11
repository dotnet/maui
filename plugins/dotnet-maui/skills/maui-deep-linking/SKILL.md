---
name: maui-deep-linking
description: >
  Guide for implementing deep linking in .NET MAUI apps. Covers Android App Links
  with intent filters, Digital Asset Links, and AutoVerify; iOS Universal Links with
  Associated Domains entitlements and Apple App Site Association files; custom URI
  schemes; and domain verification for both platforms.
  USE FOR: "deep linking", "app links", "universal links", "custom URI scheme",
  "intent filter", "Associated Domains", "Digital Asset Links", "open app from URL",
  "handle incoming URL", "domain verification".
  DO NOT USE FOR: in-app Shell navigation (use maui-shell-navigation),
  push notification handling (use maui-push-notifications), or web content embedding (use maui-hybridwebview).
---

# .NET MAUI Deep Linking

Use this skill when adding deep link or app link support to a .NET MAUI application.

## Android App Links

### IntentFilter on MainActivity

```csharp
[IntentFilter(
    new[] { Android.Content.Intent.ActionView },
    Categories = new[] {
        Android.Content.Intent.CategoryDefault,
        Android.Content.Intent.CategoryBrowsable
    },
    DataScheme = "https",
    DataHost = "example.com",
    DataPathPrefix = "/products",
    AutoVerify = true)]
public class MainActivity : MauiAppCompatActivity { }
```

- `AutoVerify = true` triggers Android domain verification at install time.
- Stack multiple `IntentFilter` attributes for different paths.

### Digital Asset Links (domain verification)

Host `/.well-known/assetlinks.json` on your domain over HTTPS:

```json
[{
  "relation": ["delegate_permission/common.handle_all_urls"],
  "target": {
    "namespace": "android_app",
    "package_name": "com.example.myapp",
    "sha256_cert_fingerprints": ["AA:BB:CC:..."]
  }
}]
```

Get SHA-256: `keytool -list -v -keystore my-release-key.keystore -alias alias_name`

### Handle incoming intents

```csharp
protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    HandleDeepLink(Intent);
}

protected override void OnNewIntent(Intent? intent)
{
    base.OnNewIntent(intent);
    HandleDeepLink(intent);
}

void HandleDeepLink(Intent? intent)
{
    if (intent?.Action != Intent.ActionView || intent.Data is null) return;
    Shell.Current.GoToAsync(MapToRoute(intent.Data.ToString()!));
}
```

### Test

```bash
adb shell am start -W -a android.intent.action.VIEW \
  -d "https://example.com/products/42" com.example.myapp
adb shell pm get-app-links com.example.myapp
```

---

## iOS Universal Links

### Associated Domains entitlement

In `Entitlements.plist`:

```xml
<key>com.apple.developer.associated-domains</key>
<array>
    <string>applinks:example.com</string>
</array>
```

### Apple App Site Association file

Host at `/.well-known/apple-app-site-association` (Content-Type: `application/json`):

```json
{
  "applinks": {
    "details": [{
      "appIDs": ["TEAMID.com.example.myapp"],
      "components": [{ "/": "/products/*" }]
    }]
  }
}
```

iOS 14+ fetches AASA via Apple's CDN; changes may take 24 hours to propagate.

### Handle Universal Links in MAUI

```csharp
builder.ConfigureLifecycleEvents(events =>
{
#if IOS || MACCATALYST
    events.AddiOS(ios =>
    {
        ios.FinishedLaunching((app, options) =>
        {
            var activity = options?[UIKit.UIApplication.LaunchOptionsUniversalLinkKey]
                as Foundation.NSUserActivity;
            HandleUniversalLink(activity?.WebPageUrl?.ToString());
            return true;
        });
        ios.ContinueUserActivity((app, activity, handler) =>
        {
            if (activity.ActivityType == Foundation.NSUserActivityType.BrowsingWeb)
                HandleUniversalLink(activity.WebPageUrl?.ToString());
            return true;
        });
        ios.SceneWillConnect((scene, session, options) =>
        {
            var activity = options.UserActivities?
                .ToArray<Foundation.NSUserActivity>()
                .FirstOrDefault(a =>
                    a.ActivityType == Foundation.NSUserActivityType.BrowsingWeb);
            HandleUniversalLink(activity?.WebPageUrl?.ToString());
        });
    });
#endif
});

static void HandleUniversalLink(string? url)
{
    if (string.IsNullOrEmpty(url)) return;
    MainThread.BeginInvokeOnMainThread(async () =>
        await Shell.Current.GoToAsync(MapToRoute(url)));
}
```

### Testing

- **Must test on a physical device.** Simulator does not support Universal Links.
- Verify AASA: `swcutil dl -d example.com` on macOS.

---

## Shell Navigation Integration

```csharp
// Register in AppShell constructor
Routing.RegisterRoute("products/detail", typeof(ProductDetailPage));

static string MapToRoute(string uri)
{
    var segments = new Uri(uri).AbsolutePath.Trim('/').Split('/');
    return segments switch
    {
        ["products", var id] => $"products/detail?id={id}",
        ["settings"] => "settings",
        _ => "//"
    };
}
```

## Checklist

- [ ] Android: `IntentFilter` with `AutoVerify = true` on `MainActivity`
- [ ] Android: `assetlinks.json` at `/.well-known/` with correct SHA-256
- [ ] Android: Handle intent in both `OnCreate` and `OnNewIntent`
- [ ] iOS: `applinks:` in Associated Domains entitlement
- [ ] iOS: AASA file at `/.well-known/apple-app-site-association`
- [ ] iOS: Handle via `FinishedLaunching`, `ContinueUserActivity`, `SceneWillConnect`
- [ ] iOS: Test on physical device (not simulator)
- [ ] Shell routes registered and URI-to-route mapping implemented
