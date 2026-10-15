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
Routine clipboard methods/getters/events, async text-to-speech and alert/confirm/action-sheet calls now
come directly from deliberately curated SDK contracts. Other API families remain
cohesive policy/resource plugins, with directly typed
method parameters and genuine structured options/results. Window actions and
event ownership share `WindowProvider`; menu actions and rendering share
`MenuProvider`. There are no per-API DI extensions, command-provider interfaces
or per-operation Args records. Plugin constructors and minimal host/protocol
contracts are public; feature wire DTOs and notification codecs are implementation
details.

## Separate binary transport and operation-owned channels

This WIP uses the new **opt-in Core binary primitive**, not a stock published
MAUI package capability. Before the document boots, the owning host sets
`view.BinaryInvoker = new HybridAppBinaryInvoker(hub)` alongside the existing
JSON target. This approves only `hybrid.invoke`; the adapter forwards into the
same scoped hub's session authorization, command registry, replay cache, shared
16 ordinary-operation budget, deadline and cancellation boundaries. Four
additional bounded control slots keep acknowledgements, fetches and cancellation
available when owners fill that budget. Never install an
independent Core delegate that bypasses the hub. This assignment is explicit per
view, not automatic/global opt-in.
Three ordinary binary owners leave the fourth Core transfer slot for channel
fetches; SDK control calls are serialized through a bounded 64-call queue.

The same generated assembly extension registers both action kinds:

```csharp
using Microsoft.Maui.Interop;

public sealed record ChunkInfo(int Index);
public sealed class Documents
{
    [HybridAction("documents.echo")]
    public static byte[] Echo(byte[] payload, string label) => payload;

    [HybridAction("documents.chunks")]
    public static async Task<int> Chunks(HybridChannel<byte[]> chunks,
        HybridChannel<ChunkInfo> progress, CancellationToken cancellation)
    {
        for (var i = 0; i < 4; i++)
        {
            cancellation.ThrowIfCancellationRequested();
            await chunks.SendAsync(new byte[] { 0, 128, 255 });
            await progress.SendAsync(new ChunkInfo(i));
        }
        return 4;
    }
}
```

The compiler treats a top-level `byte[]` result (including Task/ValueTask) as a
binary action; at most one nonnullable, required `byte[]` input binds the
separate payload and consumes no named/positional JSON argument. Such an input
requires a byte-result action. No Stream, base64, numeric-array serialization,
provider interface, runtime reflection or manual action mega-registration is
introduced. Structured args, services, keyed services, context, cancellation,
static/instance methods and main-thread policy retain their generated binders.
`HybridChannel<T>` parameters consume a caller channel UUID argument; the
compiler generates the typed JSON metadata for T or the raw-byte binder itself.
Application code supplies neither channel JSON metadata nor a delegate from web.

Whole buffers are limited to **4 MiB input and result**. Structured binary
metadata is a complete version-1 request with session/id/command/args, limited
to **2400 ASCII-escaped characters**, split into **1–10** consecutive
`maui-request-00` … `maui-request-09` headers of at most **240 characters**.
The adapter rejects missing, nonconsecutive, extra or invalid fragments.
This fits Core's independent 16-header/256-character value and encoded metadata
budgets (including heavily escaped text) without changing its policy. JSON commands retain the stock **100000**
request-character limit. Core additionally bounds active transfers to four and
their lifetime to 30 seconds, so a binary action cannot promise the full
five-minute interactive policy (the hub still applies that policy).
Raw results use Core's bytes response. Target exceptions remain genuine Core
failures: the adapter throws a bounded JSON-coded error message which the SDK
maps to NativeError, never returning success-shaped error bytes.

Channels are **action-owned**, not background subscriptions. At most four per
action, one send in flight per channel, 64 KiB per raw message or 16 KiB per
ASCII JSON message. Await every SendAsync; concurrent sends reject `channel_busy`
rather than accumulating an unbounded queue. Each send carries a sequence;
raw events carry only a scoped, single-consumption data UUID. JS fetches bytes
over the binary path, awaits decoding and the async consumer, then acknowledges.
Native validates session, owner request, channel, data id and sequence; early,
late, replayed and wrong-owner fetch/acks reject. Consumer exceptions send a
negative ack (`consumer_error`); unsubscribe, operation cancel/deadline, document
reload/disconnect and owner completion discard pending bytes and waiters.
Old document callbacks cannot fetch or acknowledge new document data.
Host event delivery still needs the existing UI-thread/current-session recheck.

Selected `FileResult` authority and the existing read-stream resources are reused,
not duplicated. `file.readBytes` now returns binary **one EOF byte (0/1) followed
by at most 65536 actual file bytes**; this explicit marker preserves partial,
empty and nonseekable EOF semantics. JS readBytes/readText reuse that route,
retain serialized read/seek/close, and preserve strict incremental UTF-8 decoder
state/reset behavior. Selected files remain read-only: there is no arbitrary filesystem
path or writable-file API. The separate share cache below stages only bounded
browser-produced PDF/text bytes, not arbitrary native filesystem writes.

The playground's Arguments page (`#/arguments`) demonstrates a 256 KiB Blob
roundtrip, a controlled native-produced PDF as bytes/Blob, and four ordered
64 KiB chunks with awaited consumers. It uses annotated `sample.binaryEcho`,
`sample.binaryDocument`, `sample.binaryChunks`, not fake native APIs.

## One startup registration

```csharp
using Maui.HybridApp.Generated;

builder.Services
    .AddHybridApp(new HybridAppOptions(maximumTextLength: 4096))
    .AddMyToolkitActions(); // Generated in the MyToolkit assembly.
```

`AddHybridApp(IServiceCollection, HybridAppOptions)` records typed local factories,
immutable policy, a default `TimeProvider.System` clock and **only implemented** product providers:
AppInfo, DeviceInfo, Clipboard, Preferences, Battery, Display, FilePicker resources,
Connectivity, TextToSpeech, HTTPS Launcher, native Geolocation, bounded Share and
MAUI page/window UI providers. It supplies borrowed real
Essentials interface defaults in the local overlay; host overrides are retained.
Hubs, registries, notifications, contextual objects and plugin targets are
**not registered as global host services**. Repeating equal
text/window resource policy is idempotent, without duplicate providers; conflicting
policy is rejected instead of silently advertised with different native limits.
There is no scanning, permission grant or switch enabling every Essentials API.

`AddHybridApp` calls `AddMauiHybridAppActions` once. The compiler finds every
annotated plugin in that assembly and emits one explicit extension that registers
all its local plugin factories, direct action delegates and declarative topics. External
assemblies each have their own single generated extension; importing a DLL alone
does not opt it in. The local registry is constructed **once per view overlay** from
these explicit assembly registrations. Connection metadata contains the actual
compiled command names and that same immutable policy.
Providers use existing Essentials
interfaces directly, not new service wrappers. `NativeDispatcher` is only a
main-thread/cancellation boundary; product actions use the attached Page dispatcher,
while teardown can still use that same owning Window dispatcher after logical
detachment. Explicit host/test dispatchers are preserved.
Preferences use only the closed scalar adaptation required by generic `IPreferences`:
typed string/bool/numeric/Date representations, including finite numbers and
lossless JS integer safeguards. Actual keys, strings and optional `sharedName`
pass unchanged to native storage. There is no prefix, key syntax/length restriction,
string quota, ownership index or rollback. Native `Clear(sharedName)` clears the
entire selected container, including settings created outside JavaScript.
Native errors and partial writes preserve destination behavior.
The remaining `MaximumTextLength` option defaults to 4096 UTF-16 units and applies
to clipboard/share, not speech or preferences.

`ITextToSpeech` declares `speech.locales` (MainThread) and `speech.speak`
(MainThread/Interactive) directly. The curated `HybridExport` root emits direct
async interface calls, preserving host overrides and the framework operation token.
`Task<IEnumerable<Locale>>` serializes as an array; `SpeakAsync` completes with null.
Native `SpeechOptions`/`Locale` are mapped without duplicate DTOs or accepted-result
envelopes. JSON/schema/finite representation checks remain, but native Essentials
alone validates text, pitch, volume, rate and locale semantics.

The app's `AddPlayground(...)` calls `AddMauiControlsSampleHybridWebAppActions`
once. `SamplePlugin` owns its scoped counter, echo, deliberate failures and work;
process diagnostics remain a separate app-only plugin.

### Owning scopes and native events

DI `Scoped` does not automatically mean one page/window. At the application
composition root, `App.CreateWindow` explicitly creates an `IServiceScope`,
then a `HybridServiceProvider` over that owning scope. It constructs `MainPage`
with the **local** hub/context, and disposes local resources before the host
scope on `Window.Destroying` (also on construction failure).
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
counter state; another window gets independent local instances.
Unowned ordinary/keyed lookups forward to the original scope, not a handler's
alternate scope. Generated typed constructors receive the proxy, so local plugin
constructor and method injection use the same provider. Host factories for local
targets also receive the proxy. Explicit singleton/instance overrides stay
host-owned/shared; forwarded business services are never disposed by the overlay.
Async-only owned plugins require awaited `DisposeAsync`, including concurrent
callers; never dispose their host scope before that await finishes.
The sample's `SharedCounterTarget` is explicitly supplied as a host singleton;
`LocalCounterTarget` is generated locally. `#/lifetime` compares both in child
windows and across frontend reload.
View/session-capturing targets reject global singleton overrides. Ordinary and
keyed business registrations still forward unchanged to the owning host scope.

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

## Metadata below the optional runtime

`HybridAction`, `FromServices`, `HybridTopic`, `HybridEvent`, `HybridExport` and
`HybridNativeEvent` live in **`Microsoft.Maui.Interop` in the existing Essentials
assembly**. They depend only on `System.Attribute`/`System.Type`, not Core,
Controls, DI, JSON or the hybrid runtime. There is no metadata package.
SDK annotations alone expose nothing, run no SDK generator and register no bridge.

```csharp
using Microsoft.Maui.Interop;
using Maui.HybridApp;
using Maui.HybridApp.Generated;
using Microsoft.Extensions.DependencyInjection;

// Curated member import, not "scan all loaded assemblies".
[assembly: HybridExport(typeof(Microsoft.Maui.ApplicationModel.DataTransfer.IClipboard))]

// Your source plugins can still use ordinary typed methods.
public sealed class MyActions([FromKeyedServices("business")] MyBusiness service)
{
    [HybridAction("my.count")]
    public int Count([FromServices] IServiceProvider owner) => service.Count();
}

// Configure once at startup. Do not duplicate the product's own IClipboard root
// if you also call AddHybridApp; cross-assembly conflicts are diagnosed/rejected.
builder.Services.AddMyToolkitActions();
using var hostScope = owningProvider.CreateScope();
await using var local = hostScope.ServiceProvider.CreateHybridAppProvider();
var hub = local.GetRequiredService<HybridAppTargetObject>();
```

The product imports `IClipboard`, `IGeolocation`, `IWindow` and selected annotated
`Page` overloads in one generated assembly registration. Imported interfaces and
abstract/contextual classes are never constructed. Calls resolve the exact
interface override (`GetRequiredService<IClipboard>()`), or the live nearest
logical Page/owning Window/primary HybridWebView at invocation. Detached views
and foreign windows fail `context_unavailable`; there is no MainPage fallback.
Sender-free `EventHandler<EventArgs>` change metadata owns epoch-guarded attach/
detach on first/last subscription and document retirement. Other event payloads
keep explicit scoped lifecycle plugins.

## Native JSON and deliberate adapters

Automatic scalar/DTO/default/enum/TimeSpan metadata remains reflection-free.
Accessible, parameterless, typed `JsonConverter<T>` attributes are honored by
direct construction. Complex contracts may explicitly declare
`[assembly: HybridJsonContext(typeof(T), typeof(MyJsonContext))]`, rooted in a
**source-declared** strict STJ context. Generated contexts cannot feed the STJ
generator in the same pass. Runtime registration verifies exact typed metadata,
duplicate/unknown-member rejection and nullable/constructor enforcement. Unsupported
polymorphism/property converters still diagnose unless the whole contract is
rooted in a suitable consumer context. No global mega-type registry is required.
The consumer proof roundtrips extensible `DevicePlatform.Create("CustomDesktop")`
through a consumer converter rather than a closed platform switch.

`geolocation.location` takes the actual `GeolocationRequest` and returns actual
`Location?`, using an explicit STJ context for Location. Native timeout must be
positive and at most two minutes. The native host declares Android coarse/fine
location, Apple when-in-use/temporary precise descriptions, and Windows location
capability. OS permission/settings/unsupported failures remain explicit. No
coordinates are logged by the demonstration.

`ShareProvider` remains a deliberate policy/resource adapter. It uses a real
.NET 11 `NativeShareRequest(ShareTextRequest, ShareFileRequest,
ShareMultipleFilesRequest)` union with `JsonUnionTypeStructuralClassifier`,
not an annotated open base class. Zero/multiple candidates, null/default/empty
requests and unknown members reject. Classification examines root property
names, not nested content; its nested `ShareFile` converter alone accepts an
authorized `{ "id": "opaque UUID" }`. It never accepts/serializes `FullPath`.

`share.stage` receives raw binary bytes and returns small UTF-8 staged metadata
bytes. `share.request` is a separate interactive **JSON** call, not a binary UI
wait. Only PDF/text, plain ASCII file names, 4 MiB per file, eight resources /
8 MiB per document are enabled. The shared app-owned retained cache rejects
overflow at 32 files / 64 MiB, rather than deleting young recipient files.
`share.release` revokes handle authority/capacity and removes unshared bytes;
it never deletes young OS-retained files. Unshared files are also removed on document/scope cleanup. Shared files survive
chooser task completion, reload and scope disposal for a finite 24-hour
retention from actual native completion; active native requests cannot be pruned.
The app calls `PruneHybridShareCache` at startup without creating a hub; subsequent
staging/share calls and cache activation also prune expired files. There is no permanent storage
promise and no proof recipients have consumed a file. Native Essentials still
owns platform URI/read grants. Parent native acceptance must inspect actual
chooser/recipient behavior, not a browser download.

### Interactive OS UI and read-only device data

`[HybridAction(..., Interactive = true)]` opts native OS interactions into the extended budget. The
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
The product has no platform-specific battery adapter. The Catalyst IOKit fix is
in Essentials itself, so the same `IBattery` read is used on every platform.
Essentials now supplies actual Mac capacity/state/power and native change
notifications instead of UIKit's incorrect Catalyst value. Unit regressions and
the plain Essentials sample verify this independently of the bridge.
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
`MenuProvider` validates and renders TypeScript-defined menu descriptions, owns
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
using Microsoft.Maui.Interop;
using Maui.HybridApp.Generated;
using Microsoft.Maui.ApplicationModel.DataTransfer;

// This application's assembly is named MyHost.
builder.Services
    .AddHybridApp(new HybridAppOptions())
    .AddMyHostActions();

public sealed record EchoMessage(string Text, int Count);

// A normal nonpartial class. Its typed constructor is registered locally automatically.
public sealed class MyActions(IClipboard clipboard)
{
    [HybridAction("app.echo")]
    public EchoMessage Echo(string text, int count, int increment = 1) =>
        new(text, count + increment);

    [HybridAction("app.clipboardText", MainThread = true)]
    public Task<string?> ReadClipboard() => clipboard.GetTextAsync();
}
```

`app.echo` accepts either `{"text":"hello","count":0,"increment":2}`
or `["hello",0,2]`. Omitted optional parameters use their C#
defaults. Explicit `null` is allowed only where the declared parameter/member
allows it. Envelopes must be named objects or positional arrays: missing required,
unknown, duplicate, excess and malformed arguments fail with `invalid_arguments`
before invoking native code. DTO JSON names use camelCase, with explicit
`JsonPropertyName` support. Ordinary method parameter names are the exact object keys.
Automatic enum metadata uses strict camel-case member names (flags use arrays of
names), not numeric JSON. The declared Location context follows the same policy
with an AOT-safe generic string-enum converter and integer values disabled.

Methods may be public or internal, static or instance, on accessible nonpartial
classes. No registration interface or partial class is required. Instance
actions call `GetRequiredService<T>()` on the owning local proxy at invocation, never a
cached root provider. The generated assembly extension is idempotent and preserves
existing explicit plugin factories. Singleton overrides are allowed for explicitly
shared business targets, not targets whose typed constructors capture a Page,
Window, dispatcher, hub, view context or notification source. The generator marks
those targets as per-view-only; the overlay rejects accidental singleton capture.
Host singleton/instance targets remain externally owned.
Static-only methods need no registered provider instance or constructor dependencies.
Fully static classes are composed by the same assembly extension, not another
per-type registration method. Extension names strip non-letter/digit characters
from the assembly name: `My.Toolkit` emits `AddMyToolkitActions` in
`Maui.HybridApp.Generated`.

Mark method parameters with **`Microsoft.Maui.Interop.FromServices`** or the existing
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
fallback. Typed parameterless converters and declared strict source contexts
are supported as described above. Unrooted polymorphism/property converters,
`object`/`dynamic`, delegates,
multidimensional arrays, unsupported framework contracts and inaccessible or
unconstructible DTO shapes receive compile-time diagnostics. Read-only members
must bind to constructor parameters; optional init-only members must also bind
to a constructor. Required writable members and optional constructor defaults
are enforced without removing constructor/property initializers.

### Native enums and wire names

Action arguments/results use actual native enum types wherever MAUI exposes one:
`FlowDirection`, `KeyboardFlags`, `AppTheme`, battery/power/energy-saver enums,
network access/profiles and display orientation/rotation. Native speech also uses
`SpeechOptions` and `Locale` directly; constructor defaults and null-to-empty locale
normalization remain native behavior. The SDK retains small enums only where the
native API uses different objects/classes: menu kinds select native menu classes, and portable menu `Primary`
still maps to Cmd on Mac Catalyst or Ctrl on Windows. Preference primitive kinds
and browser actions/statuses are closed SDK enums, not arbitrary strings.

Generated enum codecs are reflection-free, exact **camelCase name strings**, never
numeric casts: `AppTheme.Unspecified` is `"unspecified"` (OS following),
`BatteryState.NotCharging` is `"notCharging"`, `BatteryPowerSource.AC` is `"ac"`,
`ConnectionProfile.WiFi` is `"wiFi"` and `DisplayRotation.Rotation90` is
`"rotation90"`. Unknown names, wrong casing, numeric JSON and undeclared numeric strings
reject before the native method is called (explicit member-name overrides are
declared names, not numeric coercion). Undefined enum values returned by
native code fail serialization rather than leaking numbers. Nullable enums and
optional parameter/constructor defaults retain the same name policy.
`JsonStringEnumMemberName` overrides are honored verbatim. All aliases are accepted;
the ordinal-first C# member name for a numeric value determines its canonical
output. Conflicting wire names for different values are a `HYBRID004` diagnostic.

`[Flags]` enums use **arrays of exact names**, for example
`keyboard: { flags: ["spellcheck", "suggestions"] }`. Empty arrays mean zero; named zero
and all-bits sentinels must stand alone (`["none"]`, `["all"]`).
Duplicate values, including alias duplicates, reject. Exact declared composites
use one canonical name; other expressible combinations use deterministic
ascending-bit-value names. Signed/high-bit values and `All = ~0` are supported,
but All does **not** authorize otherwise undeclared bits. Undefined or
unexpressible output combinations fail clearly. Menu modifiers remain arrays
of individual SDK enum values with additional native platform/duplicate-flag checks.

Scoped theme, connectivity and browser event contexts use AOT-safe generic
`JsonStringEnumConverter<T>` with camelCase names and integer values disabled,
matching their generated action-result names. Current event contracts contain
no flags; any future flag-bearing event must also use the array policy, not STJ's
comma-separated flag string default. Browser `status` is closed
(`closed/loading/loaded/failed/blocked`); separate nullable `error` text and native
`WebNavigationResult` fields preserve navigation detail without overloading it.

`DevicePlatform` is an extensible value struct, **not** a closed enum. It remains
an extensible string here. File names/keys, text, errors, MIME types and similar
open values remain strings; this does not authorize serializing native resources
or arbitrary object graphs. Custom `JsonConverter` binding and structural unions
require explicit strict declared metadata, not an implicit relaxation of generated contracts.

All four Page dialogs, including `DisplayPromptAsync`, are directly annotated and
imported through the existing Page root. There is no prompt request/result
adapter or manually registered action lambda: prompt returns native `string | null`.
Labels, negative maxLength, initial text and action-sheet button counts pass to
Page without additional host semantics. Owning context, main-thread dispatch,
interactive serialization and cancellation bookkeeping remain in the generated
invocation path.

`Keyboard` is a native class, not an enum. `NativeKeyboardJsonContext` explicitly
roots a reusable typed `JsonConverter<Keyboard>` with the normal `HybridJsonContext`
mechanism; the generator has no keyboard special case. Exactly one JSON property
selects a representation: `{ "name": "email" }` maps to the actual known singleton,
or `{ "flags": ["spellcheck", "suggestions"] }` calls `Keyboard.Create` with actual
KeyboardFlags. Null/omission leaves Page's default; all eleven singleton names
round-trip by identity, including Plain versus a newly created flag-free keyboard.
The typed flags converter retains the strict camelCase array convention above.
Unknown/duplicate names, invalid shapes and extra/mixed properties reject only
as representation errors. No Core Keyboard API changes are required.

| Diagnostic | Contract failure |
| --- | --- |
| `HYBRID001` | Invalid bounded ASCII action name |
| `HYBRID002` | Inaccessible, generic, by-reference, delegate or async-void method |
| `HYBRID003` | Duplicate names across the whole assembly or known opted-in assemblies |
| `HYBRID004` | Unsupported automatic JSON contract |
| `HYBRID005` | Conflicting service/context/cancellation binding |
| `HYBRID006` | Invalid or unpaired first/last event subscription hooks |

There is no module initializer, per-type global slot, runtime assembly scan,
method discovery, `MethodInfo` dispatch or expression compilation. Each opted-in
assembly ships its own generated bindings and wire-name diagnostic manifest.
Runtime registry construction additionally rejects conflicts across assembled
registrations before the hub starts its lease timer. The existing Core
HybridWebView invocation generator is not replaced or widened.

All implemented product and sample actions use annotations, including internal
session controls. SDK wrappers send named arguments matching the method's
ordinary parameters. Preferences use `get/set/contains/remove/clear` with native
primitives; old string `read/write/delete` commands are removed. Single-file
selection returns nullable metadata directly, not an outer `{ file }` wrapper.
This WIP has no compatibility aliases; deploy the matching native and JS versions
together. Namespace limits, preference reservation/rollback and interactive
policy remain intact. FilePicker keeps document-owned `FileResult`
handles and owned read streams: reader locks serialize read/seek, reader lifetime
cancellation stops active reads, rejected/late opens dispose their streams, and
reader close/file release/document retirement/scope disposal release resources.
Picker cancellation stops the response but cannot force-dismiss stock OS UI;
the native interaction slot remains owned until actual dismissal.

### Declarative event ownership

Use `[HybridTopic("operation.progress")]` on a plugin class for an operation or
native callback-published topic that has no attach/detach subscription hooks.
OS source ownership uses paired explicit attributes on the **same** plugin:

```csharp
[HybridEvent("clipboard.changed", HybridEventLifecycle.Start)]
internal Task StartAsync() { /* dispatch and attach this scope's handler */ return Task.CompletedTask; }

[HybridEvent("clipboard.changed", HybridEventLifecycle.Stop)]
internal void Stop() { /* invalidate queued delivery before dispatching detach */ }
```

The start hook must return `Task`; the stop hook must be synchronous `void`;
both are accessible instance methods with no parameters. Generated bindings
capture the current scoped source during registry composition, but attach OS
handlers only on the first subscription. This also lets disposal call Stop after
DI has disabled service resolution. Source constructors depend on host/scoped
infrastructure, not the hub being constructed. Bindings never cache a root
provider or invent event/resource ownership. Stop must invalidate queued callbacks before
posting native teardown and must not synchronously wait on the hub while holding
a plugin gate. Native payload/progress codecs may use a small source-generated
context, with strict nullable/required/unknown-member options; action arguments
and results require no manual metadata lists. See `Tests/ExternalProvider` for
separate-assembly package-consumer evidence.

`[HybridAction(..., Enabled = false)]` returns `disabled_command`, not a permission
error. Hosts remain responsible
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
