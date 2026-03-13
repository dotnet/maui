# .NET MAUI Copilot Skills Plugin

A collection of **46 skills** and a **builder agent** for .NET MAUI cross-platform app development, designed for use with GitHub Copilot CLI, VS Code, and Claude Code.

Skills are loaded on-demand when your prompt matches the skill's topic, injecting expert-level guidance, code examples, and platform-specific notes into the AI's context.

> [!NOTE]
> This plugin is intended for **.NET MAUI app developers** (people building apps with the framework). For contributing to the .NET MAUI framework itself, see the [contributor skills](../../.github/skills/) in the repository root.

## Installation

### GitHub Copilot CLI

```bash
/plugin marketplace add dotnet/maui
/plugin install dotnet-maui@dotnet-maui
/skills reload
```

### Claude Code

```bash
/plugin marketplace add dotnet/maui
/plugin install dotnet-maui@dotnet-maui
```

### VS Code

Skills become available automatically when working in a repository that references this plugin.

### Manual

Each skill is a self-contained directory with a `SKILL.md` file using standard YAML frontmatter. Copy the skill directories into whichever location your AI tool reads skills from.

## Builder Agent

The **MAUI App Builder** agent (`agents/maui-app-builder.md`) orchestrates multiple skills for complex, multi-step requests:

- *"Build me a two-tab app with a list page and settings"*
- *"Add MVVM, dependency injection, and Shell navigation to my project"*
- *"Convert my single-page app to use Shell with flyout menu"*

The agent enforces best practices, prevents obsolete patterns, and applies skills in the correct dependency order.

## Skill Catalog

### Scaffolding
| Skill | Description |
|-------|-------------|
| [scaffold-page](skills/scaffold-page/) | Create a new ContentPage with XAML, code-behind, namespace derivation, and optional ViewModel wiring |
| [scaffold-content-view](skills/scaffold-content-view/) | Create reusable ContentView components with BindableProperty declarations |

### Architecture & Patterns
| Skill | Description |
|-------|-------------|
| [setup-mvvm](skills/setup-mvvm/) | Set up MVVM with ViewModels, commands, CommunityToolkit.Mvvm, and DI registration |
| [maui-data-binding](skills/maui-data-binding/) | XAML data bindings, compiled bindings, value converters, binding modes, and MVVM practices |
| [maui-dependency-injection](skills/maui-dependency-injection/) | DI in MauiProgram.cs — service registration, lifetimes, constructor injection, platform-specific registration |
| [maui-shell-navigation](skills/maui-shell-navigation/) | Shell navigation — tabs, flyout, URI-based GoToAsync, route registration, query parameters |
| [maui-custom-handlers](skills/maui-custom-handlers/) | Custom handlers, property/command mappers, platform-specific native views |
| [maui-platform-invoke](skills/maui-platform-invoke/) | Platform-specific native APIs — partial classes, conditional compilation, multi-targeting |
| [maui-app-lifecycle](skills/maui-app-lifecycle/) | App lifecycle — states, Window events, backgrounding, resume, state preservation |

### Guardrails
| Skill | Description |
|-------|-------------|
| [maui-coding-guardrails](skills/maui-coding-guardrails/) | Always-on guardrail preventing obsolete controls (ListView, TableView), deprecated patterns, layout mistakes |
| [maui-current-apis](skills/maui-current-apis/) | Always-on guardrail for API currency — prevents use of removed/deprecated APIs in .NET MAUI 10 |

### UI & Controls
| Skill | Description |
|-------|-------------|
| [maui-collectionview](skills/maui-collectionview/) | CollectionView — layouts, selection, grouping, templates, incremental loading, swipe actions |
| [maui-animations](skills/maui-animations/) | View animations, custom animations, easing, rotation, scale, translation, fade |
| [maui-gestures](skills/maui-gestures/) | Tap, swipe, pan, pinch, drag-and-drop, and pointer gesture recognizers |
| [maui-graphics-drawing](skills/maui-graphics-drawing/) | Custom drawing with Microsoft.Maui.Graphics, GraphicsView, canvas operations |
| [maui-theming](skills/maui-theming/) | Theming — light/dark mode, AppThemeBinding, dynamic resources, ResourceDictionary |
| [maui-safe-area](skills/maui-safe-area/) | Safe area and edge-to-edge layout for .NET 10+ |
| [maui-app-icons-splash](skills/maui-app-icons-splash/) | App icons, splash screens, SVG→PNG conversion, platform-specific requirements |

### Platform APIs & Services
| Skill | Description |
|-------|-------------|
| [maui-geolocation](skills/maui-geolocation/) | GPS/location — one-shot and continuous, permissions, accuracy, mock detection |
| [maui-secure-storage](skills/maui-secure-storage/) | Secure key-value storage — Keychain (iOS), Keystore (Android), DPAPI (Windows) |
| [maui-media-picker](skills/maui-media-picker/) | Photo/video picking and camera capture, multi-select (.NET 10), permissions |
| [maui-file-handling](skills/maui-file-handling/) | File picker, file system helpers, bundled assets, app data storage |
| [maui-permissions](skills/maui-permissions/) | Runtime permissions — check, request, custom permissions, platform manifests |
| [maui-local-notifications](skills/maui-local-notifications/) | Local notifications — channels, scheduling, foreground/background, platform setup |
| [maui-push-notifications](skills/maui-push-notifications/) | Push notifications — FCM (Android), APNs (iOS), Azure Notification Hubs |
| [maui-speech-to-text](skills/maui-speech-to-text/) | Voice input via CommunityToolkit.Maui speech recognition |
| [maui-maps](skills/maui-maps/) | Map controls, pins, polygons, geocoding, Google Maps API setup |

### Data & Networking
| Skill | Description |
|-------|-------------|
| [maui-rest-api](skills/maui-rest-api/) | REST API consumption — HttpClient, System.Text.Json, DI, CRUD, error handling |
| [maui-sqlite-database](skills/maui-sqlite-database/) | Local SQLite storage — sqlite-net-pcl, async service, WAL mode, DI |
| [maui-authentication](skills/maui-authentication/) | Authentication — WebAuthenticator OAuth, MSAL.NET for Entra ID, broker support |

### Web & Hybrid
| Skill | Description |
|-------|-------------|
| [maui-hybridwebview](skills/maui-hybridwebview/) | HybridWebView — JavaScript↔C# interop, bidirectional messaging, trimming |
| [maui-deep-linking](skills/maui-deep-linking/) | Deep linking — Android App Links, iOS Universal Links, custom URI schemes |
| [maui-aspire](skills/maui-aspire/) | .NET Aspire integration — AppHost, service discovery, HttpClient DI, Entra ID auth |

### Quality & Diagnostics
| Skill | Description |
|-------|-------------|
| [maui-accessibility](skills/maui-accessibility/) | Accessibility — SemanticProperties, screen readers, AutomationProperties, focus |
| [maui-performance](skills/maui-performance/) | Performance — profiling, compiled bindings, layout efficiency, trimming, NativeAOT |
| [maui-unit-testing](skills/maui-unit-testing/) | xUnit testing — ViewModel testing, mocking, test project setup, code coverage |
| [maui-hot-reload-diagnostics](skills/maui-hot-reload-diagnostics/) | Hot Reload troubleshooting — C#, XAML, Blazor Hybrid, VS/VS Code setup |
| [maui-localization](skills/maui-localization/) | Localization — .resx files, culture switching, RTL, platform language config |

### Native Bindings
| Skill | Description |
|-------|-------------|
| [android-slim-bindings](skills/android-slim-bindings/) | Android native bindings via NLI — Java/Kotlin wrappers, Gradle, Maven, AAR/JAR |
| [ios-slim-bindings](skills/ios-slim-bindings/) | iOS native bindings via NLI — Swift wrappers, Xcode, CocoaPods/SPM, xcframeworks |

### Tooling & Release Management
| Skill | Description |
|-------|-------------|
| [maui-workload-discovery](skills/maui-workload-discovery/) | .NET SDK versions, workload sets, Xcode/JDK requirements, manifest versions |
| [maui-release-notes](skills/maui-release-notes/) | Generate and maintain formatted release notes for MAUI workload releases |

### Migration
| Skill | Description |
|-------|-------------|
| [xamarin-forms-migration](skills/xamarin-forms-migration/) | Migrate Xamarin.Forms → .NET MAUI — project conversion, namespaces, handlers |
| [xamarin-android-migration](skills/xamarin-android-migration/) | Migrate Xamarin.Android → .NET for Android — SDK-style project, TFMs, manifests |
| [xamarin-ios-migration](skills/xamarin-ios-migration/) | Migrate Xamarin.iOS/Mac/tvOS → .NET — project conversion, Info.plist, code signing |

## Attribution

This plugin consolidates skills from multiple sources:

- **[davidortinau/maui-skills](https://github.com/davidortinau/maui-skills)** — 37 app development skills (MIT License)
- **[Redth/maui-skillz](https://github.com/Redth/maui-skillz)** — Native bindings, workload discovery, and release notes skills (MIT License)
- **[github/awesome-copilot](https://github.com/github/awesome-copilot)** — MAUI expert agent definition used as basis for coding guardrails and builder agent

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version matching your target MAUI release)
- .NET MAUI workload installed (`dotnet workload install maui`)
- Platform SDKs as needed (Xcode for iOS/Mac, Android SDK, Windows SDK)

## License

[MIT](../../LICENSE.txt)
