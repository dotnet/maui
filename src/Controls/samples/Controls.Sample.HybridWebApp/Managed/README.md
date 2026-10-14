# Maui.HybridApp — experimental managed product

This independently packable class library is a **WIP under the sample tree**,
not app code copied into an assembly and not an official framework package.
`Maui.HybridApp` / `0.1.0-preview.1` is an unverified working NuGet identity;
name availability, ownership and publication authorization have not been checked.
Local `dotnet pack` is permitted. Registry publication is a separate maintainer
decision and has not occurred.

## Product and host boundary

The library owns `CommandRegistry`, the bounded per-WebView session hub,
`IHybridAppTarget.Invoke(string)`, protocol/control metadata and independently
registered Essentials and MAUI UI providers. By the current product decision,
these stay in this **one** WIP product, which now references Essentials and
Controls.Core (and Core transitively), plus the standard DI abstractions supplied
by .NET 11. There is no second integration NuGet. The product never references
the sample namespace, MauiProgram, its counter/diagnostics/catalog, development
forwarding or test doubles. A future Essentials-only split is deferred.
Each API is one cohesive `Essentials/*Provider.cs` file containing its provider,
related small DTOs, DI extension and private generated JSON context. Unrelated
APIs never share a contracts/provider file. Existing namespaces/types are unchanged.
Provider constructors, DI extensions and the minimal host/protocol contracts are public;
feature wire DTOs and contexts are implementation details.

## One startup registration

```csharp
builder.Services
    .AddHybridApp(new HybridAppOptions("my-app.hybrid.", maximumTextLength: 4096))
    .AddHybridProvider<MyToolkitProvider>();
```

`AddHybridApp(IServiceCollection, HybridAppOptions)` registers the scoped
`HybridAppTargetObject`, immutable policy, scoped `NativeDispatcher`, default
`TimeProvider.System` clock and **only implemented** product providers:
AppInfo, DeviceInfo, Clipboard, Preferences, Battery, Display, FilePicker resources,
Connectivity, TextToSpeech, HTTPS Launcher and MAUI page/window UI providers. It supplies their real
existing Essentials interface defaults with
`TryAdd`; host overrides registered before it are retained. Repeating equal
namespace/text policy is idempotent, without duplicate providers; conflicting
policy is rejected instead of silently advertised with different native limits.
There is no scanning, permission grant or switch enabling every Essentials API.

The hub receives `IEnumerable<IHybridCommandProvider>` and constructs/registers
its own exclusive command registry **once per scope**. Connection metadata
contains actual registered command names and that same immutable policy.
Providers use existing Essentials
interfaces directly, not new service wrappers. `NativeDispatcher` is only a
main-thread/cancellation boundary; its default delegates to Essentials MainThread.
The app must supply its nonempty preference prefix; no sample prefix or default
entire-store namespace is embedded in the product. Text defaults to 4096 UTF-16 units; keys remain 1–48 ASCII
letters, digits, underscores or hyphens. Typed string/bool/numeric/Date values
and optional host-isolated shared containers are supported; clear removes only
tracked keys in that host namespace/container, never an entire native store.
Typed values are validated before reserving the bounded ownership index; the
index is reserved before native writes, and deletion releases its capacity.
Failed writes of newly created keys clean up the value before untracking it.
An existing value from an older preview without an index is never deleted by
that rollback; ownership is retained after a failed update. An OS write that
partially changed an existing value cannot generally be undone.

The app adds its own `AddPlayground(...)` registration for sample-only commands,
shared scoped counter and process diagnostics. Each small sample provider lives
in one flat `Native/Playground/Sample*Provider.cs` with its related DTO/context.
The shared `SampleCounter` is a separate scoped state object used by Counter/Echo.

### Owning scopes and native events

DI `Scoped` does not automatically mean one page/window. At the application
composition root, `App.CreateWindow` explicitly creates an `IServiceScope`,
resolves `MainPage` from it, and disposes it on `Window.Destroying` (also on
construction failure). This scope owns the hub, timer and disposable providers.
Do not resolve the hub from the root container or share it across WebViews.
Construct a NavigationPage wrapper before assigning Window.Page. Assigning a
ContentPage to a Window and then wrapping that same page can detach its logical
parent. Context binding verifies Page.Window, not only NavigationPage.CurrentPage.
The framework's native-context scopes are not a reason to assume this overridden
page construction automatically receives a matching scope.

`MainPage(HybridAppTargetObject, HybridViewContext)` contains only view integration, not registry or
provider factories. It subscribes to the hub's typed `NativeEventReceived` event,
marshals to the UI thread and rechecks both its stopped state and current document
before sending. `Stop` detaches host event/view handlers and Development resources;
the owning scope disposes the hub. Handler replacement disconnects its document
without disposing the still-live scope. Frontend reload keeps scoped providers and
counter state; another window gets independent instances.

The app binds `HybridWindowHost` once to its own application, a scope-owning
factory accepting a validated bundle-relative URL/query/fragment. It has no
route catalog, Svelte page knowledge or sample landing-page default. It binds
each independent `HybridViewContext` to its page/window before attaching native
handlers. Context construction does not resolve a page or hub, so there is no
page/hub/provider factory cycle. Window commands own title and desktop bounds;
the app does not hide those operations in sample providers.
Apple window opening checks UIApplication's actual multiple-scene support
before creating a child. A compiled single-scene app reports
`configuration_required`; source Info.plist contents alone are not capability
evidence.

Hub disposal is idempotent: it cancels work, retires the document, disposes its
`TimeProvider` lease timer and clears all native-event listeners. There is no page
callback factory, page/hub DI cycle or `BuildServiceProvider` call in app startup.

### Interactive OS UI and read-only device data

Registry metadata opts dialogs and FilePicker into `interactive: true`. The
handshake advertises those exact registered commands and the 300000 ms native
budget. Only these commands extend the normal 20-second native / 23-second
client deadlines. Their active operation holds the 15-second document heartbeat
lease while OS UI pauses the frontend. Completion grants a new bounded lease.
Disconnect, explicit cancellation and scope/window disposal cancel the response
wait immediately, even when the stock OS UI has not returned.

MAUI's public alert/prompt/picker APIs have no force-dismiss operation or
cancellation token. The context retains an interaction slot until actual OS
dismissal to avoid overlapping dialogs; cancellation cannot undo a completed
OS side effect. Dialogs use the calling page's `DisplayAlertAsync` and
`DisplayPromptAsync`, never a global MainPage. FilePicker uses `IFilePicker`
and `FileResult.OpenReadAsync`, never FullPath. Single/multiple selection creates
metadata/resources without reading contents. Separate native readers return
bounded binary chunks (at most 65536 bytes per message), support actual seek where
available and release on close/selection release/document teardown. This is not
a whole-file size/type cap. Incremental strict UTF-8 decoding is in the SDK;
non-UTF8 is explicit and binary bytes remain available. Picker dismissal
is normal cancellation. No file body or full path is logged by native.
Battery and Display are fresh read-only snapshots using `IBattery` and
`IDeviceDisplay`, without subscriptions or keep-screen-awake writes.
Mac Catalyst is an explicit battery-source exception: the current Essentials
UIKit path reports an incorrect fixed value on the test Mac. The product reads
actual internal battery capacities and charging/source state from IOKit, keeping
the native snapshot alive until borrowed descriptions are consumed. Capacities
are weighted across present batteries, zero charge is valid, no battery is -1,
and invalid native data is an error rather than a fabricated full charge.
This is a native adapter, not a JavaScript correction or shell command.
Android hosts must declare the Essentials `android.permission.BATTERY_STATS`
manifest permission for Battery; no runtime request or broad storage permission
is needed for these implemented read/document-picker flows.

### Desktop browser guest and menus

`HybridViewLayout` is a full-size AbsoluteLayout host, not a split row or HTML
WebView element. It owns one ordinary native WebView in a clipped container above
an explicitly measured DOM slot. The geometry contract includes full CSS bounds,
visible intersection and CSS viewport dimensions; native rescales to real host
layout units. Guest identities are owned by the calling scope. Creation is
desktop-only, initially hidden, and never attaches a command target, framework
SDK, message listener, DevelopmentSession or inspector. Top-level HTTPS policy
rejects unsafe/local/bridge destinations; it is not a DNS/subresource firewall.
Native resize hides stale geometry. Feature/document/scope teardown removes it.

The host uses NavigationPage with its unneeded navigation bar hidden so the
calling ContentPage's MenuBarItems have supported native hosting. Product
`HybridMenus` validates and renders TypeScript-defined menu descriptions, owns
actual native controls and emits installation/item identities. Every sample menu
definition/action is in `Web/src/native/menus.ts`, not a C# action switch.
Item patches retain installation/window/document ownership and callbacks.
Mac Catalyst's focused page renders in the application menubar; its actual
UIKit checks are supported, while Windows MAUI check items reject explicitly.
Native OS effects use a scoped typed notification
source, current document/topic/subscription checks and independent event UUIDs,
not operation progress IDs or JavaScript evaluation. OS callbacks have a bounded
five-minute lease hold; old callbacks cannot extend a replacement document.

Application theme uses UserAppTheme/RequestedTheme/RequestedThemeChanged, not a
per-window theme. Native effective callbacks repaint the trusted frontend CSS;
remote sites are not forced into that theme. Window geometry callbacks coalesce
actual frame/property changes. Centering supports Windows current-monitor work
areas and Mac global CoreGraphics display rectangles with the scene's actual
system frame. It requests MAUI native X/Y; it does not guess the primary screen.
Position observation remains Windows-only; mobile centering is unsupported.

## External providers

### Attributed actions

```csharp
using Maui.HybridApp;
using Microsoft.Maui.ApplicationModel.DataTransfer;

builder.Services
    .AddHybridApp(new HybridAppOptions("my-app.hybrid."))
    .AddHybridActions<MyActions>();

public sealed record EchoMessage(string Text, int Count);

// A normal nonpartial class. Instance providers are registered scoped automatically.
public sealed class MyActions(IClipboard clipboard)
{
    [HybridAction("app.echo")]
    public EchoMessage Echo(EchoMessage message, int increment = 1) =>
        message with { Count = message.Count + increment };

    [HybridAction("app.clipboardText", MainThread = true)]
    public Task<string?> ReadClipboard() => clipboard.GetTextAsync();
}
```

`app.echo` accepts either `{"message":{"text":"hello","count":0},"increment":2}`
or `[{"text":"hello","count":0},2]`. Omitted optional parameters use their C#
defaults. Explicit `null` is allowed only where the declared parameter/member
allows it. Envelopes must be named objects or positional arrays: missing required,
unknown, duplicate, excess and malformed arguments fail with `invalid_arguments`
before invoking native code. DTO JSON names use camelCase, with explicit
`JsonPropertyName` support. Ordinary method parameter names are the exact object keys.
Enums use their numeric JSON representation, matching the default native serializer.

Methods may be public or internal, static or instance, on accessible nonpartial
classes. The opt-in requires no `IHybridCommandProvider` implementation. Instance
actions call `GetRequiredService<T>()` on the owning scope at invocation, never a
cached root provider. `AddHybridActions<T>()` is idempotent and preserves an
existing explicit provider factory and its lifetime. Use scoped registrations
for native/resource providers; deliberately choosing a singleton shares that
action instance across windows, just like ordinary .NET DI.
Static-only methods need no registered provider instance or constructor dependencies.
For a fully static class (which C# forbids as a generic type argument), the analyzer
emits `Add<ClassName>HybridActions()` in `Maui.HybridApp.Generated`; import that
namespace and call the generated extension instead.

Mark method parameters with **`Maui.HybridApp.FromServices`** or the existing
`Microsoft.Extensions.DependencyInjection.FromKeyedServices` to inject services
from the same current scope. There is no fake Microsoft DI attribute or ASP.NET
dependency. `CancellationToken` always comes from the current `Operation`.
An optional `HybridActionContext` supplies the current request ID, document
session, cancellation and typed `Publish(topic, value, sourceGeneratedTypeInfo)`
for operation progress. Service/context/token parameters consume neither object
keys nor array positions. Missing required services remain native failures,
not JSON arguments or silently substituted defaults.

Supported return shapes are `void`, `T`, `Task`, `Task<T>`, `ValueTask` and
`ValueTask<T>`; no-value results serialize as JSON null. Coded `CommandException`
errors retain the existing protocol error code. `MainThread=true` dispatches
exactly once through the owning `NativeDispatcher`; unmarked synchronous work
runs on its caller, with no automatic `Task.Run`. `Interactive=true` applies the
existing advertised 300000 ms native interaction budget, not an invented timeout.
Cancellation is checked before and after invocation and does not undo native writes.

The bundled analyzer generates **direct typed calls and its own
`JsonSerializerContext`/`JsonMetadataServices` converters** for the discovered
argument/result graph. It does not expect the System.Text.Json generator to
process another generator's added `[JsonSerializable]` types in the same pass.
Scalars, nullable values, concrete DTO constructors/records, enums, one-dimensional
arrays, generic lists/sets and string-keyed dictionaries have automatic strict
metadata. There is no user-maintained action DTO registry or reflection JSON
fallback. Custom converters, polymorphism, `object`/`dynamic`, delegates,
multidimensional arrays, unsupported framework contracts and inaccessible or
unconstructible DTO shapes receive compile-time diagnostics. Read-only members
must bind to constructor parameters; optional init-only members must also bind
to a constructor. Required writable members and optional constructor defaults
are enforced without removing constructor/property initializers.

| Diagnostic | Contract failure |
| --- | --- |
| `HYBRID001` | Invalid bounded ASCII action name |
| `HYBRID002` | Inaccessible, generic, by-reference, delegate or async-void method |
| `HYBRID003` | Duplicate names in a provider or statically known host opt-ins |
| `HYBRID004` | Unsupported automatic JSON contract |
| `HYBRID005` | Conflicting service/context/cancellation binding |

Per-provider module initialization installs only its compiled registration into
its generic slot. Host opt-in is still explicit; initialization performs no
assembly scan, runtime method discovery, `MethodInfo` or expression compilation.
External assemblies ship their generated binding themselves. Registrations
composed dynamically across assemblies are additionally checked for duplicate
commands at registry construction. The existing Core HybridWebView invocation
generator is not replaced or widened.

Clipboard, Preferences and FilePicker command handlers now use annotations,
preserving every existing JS command name, input/result shape, text/key limits,
namespace, preference ownership reservation/rollback and interactive policy.
Their existing public constructors and direct `Register(registry)` composition
remain usable. Clipboard explicitly implements `IHybridActionSource` for its
manual topic/start/stop lifecycle; resource and native event ownership are not
invented by action generation. FilePicker keeps document-owned `FileResult`
handles and owned read streams: reader locks serialize read/seek, reader lifetime
cancellation stops active reads, rejected/late opens dispose their streams, and
reader close/file release/document retirement/scope disposal release resources.
Picker cancellation stops the response but cannot force-dismiss stock OS UI;
the native interaction slot remains owned until actual dismissal.

### Manual command and source providers

A Toolkit assembly implements `IHybridCommandProvider.Register(registry)` or
calls `registry.Register<TArgs,TResult>(name, inputInfo, outputInfo, handler)`.
The host opts in with `AddHybridProvider<TProvider>()` after `AddHybridApp`, or
explicit registry composition in a custom native host. The generic hub itself
does not choose Essentials/sample providers. Providers own their DTOs, generated contexts and
topics (`RegisterTopic`). The manual path needs no attributes, scanning, reflection
router, catalog or edits to the protocol context. Duplicate names/topics are rejected.
Metadata must originate from a source-generated context with
`UnmappedMemberHandling=Disallow`, `RespectNullableAnnotations=true` and
`RespectRequiredConstructorParameters=true`; use camelCase on the wire.
An explicitly disabled command returns `disabled_command`, not a permission error.
See the separate `Tests/ExternalProvider` assembly for an executable example.

The generic registry/hub does not choose any provider. Hosts remain responsible
for trusted content, allowed origins, opt-in capabilities, OS permissions and
platform configuration. There is no browser fallback. Cancellation is checked
before/after OS side effects, but does not undo a completed write. Denied,
unsupported, configuration-required and unexpected native errors are explicit,
logged failures, never success-shaped emulation.

## Build and local packaging

From the bundle root with the repository-pinned .NET 11 SDK:

```sh
# In the MAUI checkout: managed-only target for tests and local package checks.
bash ../../../../eng/common/dotnet.sh test Tests/Playground.UnitTests.csproj \
  --framework net11.0 -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false -p:IncludeMacCatalystTargetFrameworks=false \
  -p:IncludeWindowsTargetFrameworks=false -p:IncludeMacOSTargetFrameworks=false
bash ../../../../eng/common/dotnet.sh pack Managed/Maui.HybridApp.csproj \
  -c Debug -p:IncludeAndroidTargetFrameworks=false \
  -p:IncludeIosTargetFrameworks=false -p:IncludeMacCatalystTargetFrameworks=false \
  -p:IncludeWindowsTargetFrameworks=false -p:IncludeMacOSTargetFrameworks=false
```

Do not globally override TargetFrameworks in the in-tree graph: the referenced
source-generator projects require their own netstandard target during restore.
The platform-selection flags above select managed-only MAUI dependencies without
rewriting those generator targets. A copied NuGet-only project has no such
repository project-reference graph and can select its TargetFrameworks explicitly.

The full default target list includes net11.0 and the normal Android, iOS,
MacCatalyst and (on Windows) Windows TFMs, not Tizen. An in-tree build references
the actual Essentials and Controls.Core projects. App project references pass
per-platform artifact directories through the product and its dependencies;
native builds do not overwrite managed-test/package assets. No product code is source-linked.

After copying the bundle to another repository, the library's MSBuild island
never imports ancestor MAUI/sample configuration. Supply a real, compatible
`Microsoft.Maui.Essentials` and `Microsoft.Maui.Controls.Core` 11 package version
and their actual dependency feed:

```sh
dotnet restore Managed/Maui.HybridApp.csproj -p:TargetFrameworks=net11.0 \
  -p:MauiVersion=ACTUAL_VERSION --configfile NuGet.Config
dotnet pack Managed/Maui.HybridApp.csproj -p:TargetFrameworks=net11.0 \
  -p:MauiVersion=ACTUAL_VERSION --no-restore
```

There is no assumed public MAUI 11 package version. A local in-tree Essentials
package is valid evidence for offline package consumption, **not** a publicly
available feed/version. Multi-platform packing needs those installed workloads.
Native AOT certification additionally needs actual publish/device validation;
generated metadata alone is not such certification.

The NuGet contains the product DLL/XML per packed TFM, README and MIT license,
plus the bundle-local build-time generator under
`analyzers/dotnet/cs/Maui.HybridApp.Generators.dll`.
Its netstandard2.0 analyzer references Roslyn only, not the product or an AI
runtime; it does not add a second prospective managed product.
(also a portable PDB when the in-tree build includes it).
Essentials, Controls.Core and their genuine transitive dependencies are normal NuGet dependencies.
The product uses DI abstractions from the .NET 11 framework reference/runtime,
not a redundant PackageReference: such a reference causes NU1510 in a copied
standalone project. No warning/analyzer/pruning suppression is used.
The full DI container is a test/consumer dependency pinned to the repository's
`11.0.0-rtm.26480.113`, not an additional product dependency.
No app/sample/test source, web assets or runtime fakes are shipped. The separate
`Tests/PackageConsumer` executable references the **package**, uses a separately
compiled provider assembly, and disables reflection serialization. Its plugin
and host actions have no user-declared action DTO context or serialization lists:
the packed analyzer generates automatic DTO/scalar/enum/nullable/collection
metadata and scoped/keyed direct calls. It also exercises the actual one-call
registration, events, strict contracts and independent scopes. This is executable
reflection-disabled evidence, not a claim of native AOT publication certification.
Before release, inspect
the nuspec/file list, run assembly and packed-consumer tests, validate the native
platform matrix and review naming/support/signing requirements. Do not publish
this working identity without a separate authorization.

To run that consumer, supply a dedicated `NuGet.Config` whose cleared sources
contain the product package and **real** Essentials/Controls.Core/Core/Graphics/DI dependencies:

```sh
dotnet restore Tests/PackageConsumer/PackageConsumer.csproj --configfile NuGet.Config \
  --packages "$PWD/artifacts/package-consumer-cache" -p:HybridAppPackageConsumer=true
dotnet run --project Tests/PackageConsumer/PackageConsumer.csproj --no-restore \
  -p:HybridAppPackageConsumer=true \
  -p:RestorePackagesPath="$PWD/artifacts/package-consumer-cache"
```

Use a new, empty cache directory for each local proof when repacking the same
preview version. `--packages` explicitly overrides environment-level NuGet cache
settings; changing only `globalPackagesFolder` in a config is not a fresh-cache
guarantee when the pinned SDK runner supplies an environment override.

For an isolated proof, copy only `Tests/PackageConsumer`, `Tests/ExternalProvider`,
and `Tests/Directory.Build.props/targets` to
a fresh directory with a copy-local SDK pin/config. No `Managed` directory or
product project reference is needed. The normal unit tests instead reference the
real product project and a separate external provider project.
