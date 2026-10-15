# Hybrid web apps

An experimental .NET MAUI library, source generator and JavaScript SDK for apps
using the existing `HybridWebView` control. The provisional managed package,
assembly and namespace are **`Microsoft.Maui.Controls.HybridWebApps`**. The
generator ships inside that same NuGet; it is not another consumer package.
The unpublished JavaScript package is **`@maui/api`**.

Neither package name availability nor publication is claimed. This is opt-in
application-development infrastructure, not an automatic exposure of every
MAUI API. The actual control, its framework bootstrap JavaScript and the Core
binary transport stay in their existing framework projects.

## Layout

| Path | Responsibility |
| --- | --- |
| [`src/HybridWebApps`](src/HybridWebApps/README.md) | Runtime, scoped registry/session, strict JSON helpers, resource providers and one NuGet |
| `src/SourceGen` | Incremental generator, included under the runtime package's `analyzers/dotnet/cs` |
| [`JavaScript`](JavaScript/README.md) | Independent TypeScript/ESM SDK, API documentation and tarball tests |
| [`samples/HybridWebApp`](samples/HybridWebApp/README.md) | Native MAUI host, Vite/Svelte frontend and launcher |
| `tests/UnitTests` | Portable managed tests, separate plugin DLL and NuGet-only consumer |

The runtime and generator participate in `Microsoft.Maui.sln` and both package
filters. Portable managed tests are registered in `eng/helix.proj`. The sample
is in the solution, but is not in the generic samples filter: its frontend must
first be explicitly staged with npm. MSBuild validates those hashes and never
launches npm.

## Native methods and real events

```csharp
using Microsoft.Maui.Interop;

public sealed record Progress(int Completed, int Total);

public sealed class WorkService
{
    [HybridEvent("work.progress")]
    public event Action<Progress>? ProgressChanged;

    [HybridAction("work.report")]
    public void Report(int completed, int total) =>
        ProgressChanged?.Invoke(new Progress(completed, total));
}
```

Explicitly register the consuming assembly's generated `AddMyAssemblyActions()`
alongside `AddHybridApp(...)`. The actual sample uses the same direct event
pattern for `service.changed`. Curated referenced events use the same marker:
`IClipboard.ClipboardContentChanged` and
`Application.RequestedThemeChanged` need no lifecycle wrappers. The latter
serializes the real `AppThemeChangedEventArgs` as `{ requestedTheme: 'dark' }`,
not an invented snapshot DTO. `getTheme()` and `setTheme()` retain their
separate native read/write contracts.

Supported event payloads are statically typed:

* `EventHandler` / `EventHandler<EventArgs>` and zero-argument delegates publish null.
* A single parameter publishes that value; `EventHandler<T>` excludes its sender.
* A conventional `(object sender, T value)` delegate also excludes the sender.
* Multiple payload parameters publish an object with the delegate's parameter names.
* Accessible instance/static void delegates work in source classes; referenced
  classes/interfaces require an explicit `HybridExport` root.

Generic containing types, inaccessible accessors/types, nonvoid returns,
ref/out/in, pointers, ref-like and unsupported JSON contracts produce diagnostics.
This is not a claim that every possible .NET delegate can cross JSON.
Inherited hidden members bind to the annotated declaring contract.

The local document owns the first attach and last unsubscribe/disconnect/dispose
detach. Epoch guards invalidate stale delegates, including reattachment and
queued-dispatch races. The epoch is reserved atomically with hub ownership;
deferred startup and publication retain it, so a delayed start cannot revive an
unsubscribed generation. The exact receiver/handler/dispatcher are captured before
disposal; teardown never resolves disposed services. Native add/remove accessors
run outside lifecycle locks. Attach errors fail the subscription; deferred detach
and payload errors use bounded, value-free `HYBRIDWEBAPP_EVENT_ERROR` diagnostics.
Resource-specific manual Task-start/void-stop hooks remain available through
`[HybridEvent("topic", HybridEventLifecycle.Start/Stop)]`.

## Why these attributes?

Markers remain in **`Microsoft.Maui.Interop` in the existing Essentials assembly**.
Essentials cannot depend on Core/Controls or this optional accelerator, so moving
the markers upward would invert the dependency graph. There is no metadata-only
NuGet, post-initialization attribute injection or ambiguous simple-name discovery.
The generator matches exact metadata names; consumer-local matching definitions
are only a last-resort interoperability technique, not shipped duplicate types.

The BCL [`JSExport`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.javascript.jsexportattribute?view=net-11.0)
and [`JSImport`](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.javascript.jsimportattribute?view=net-11.0)
are browser-platform, method-only WebAssembly interop contracts, not native
WebView RPC/event annotations. `UnmanagedCallersOnly` requires static unmanaged
entrypoints with blittable signatures. [`JSInvokable`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.jsinterop.jsinvokableattribute?view=aspnetcore-11.0)
is method-only and belongs to Microsoft.JSInterop, not the BCL.
ASP.NET's `FromServices` also brings an inappropriate MVC dependency.
Existing `FromKeyedServices` and STJ converter/JSON attributes are reused instead
of inventing equivalents.

## Small generated bindings, compiled helpers

One assembly-pooled graph owns strict contracts, including distinct nullable
use sites and explicit consumer source contexts. Providers share primitive,
null, collection, enum/flags and subscription implementations compiled in the
runtime and independently unit tested. Generated code keeps typed member calls,
DTO construction/property access and payload adapters. It never discovers
events/methods through reflection, compiles expressions, scans assemblies or
depends on another generator processing newly generated `JsonSerializable`.

Action bindings are grouped eight per file; contract implementations are separate
files with deterministic identities. A repeated-contract fixture with eight
providers and 48 actions went from **131,347 bytes / 2,205 lines** to
**55,708 bytes / 1,057 lines** (57.6% fewer bytes, 52.1% fewer lines), measured
with the original generator normalized to the same product namespace. The
largest resulting file is 5,859 bytes. Regression tests require over 50% reduction,
bound generated files, and prove quadrupling action count leaves contract output
unchanged. This is actual deduplication, not just splitting the old output.

## Portable validation

From the repository root, using its pinned SDK:

```sh
bash eng/common/dotnet.sh test \
  src/HybridWebView/tests/UnitTests/Microsoft.Maui.Controls.HybridWebApps.UnitTests.csproj \
  -p:PublicApiType=Validate \
  -p:IncludeAndroidTargetFrameworks=false -p:IncludeIosTargetFrameworks=false \
  -p:IncludeMacCatalystTargetFrameworks=false -p:IncludeWindowsTargetFrameworks=false \
  -p:IncludeMacOSTargetFrameworks=false -p:IncludeTizenTargetFrameworks=false
npm --prefix src/HybridWebView/JavaScript run pack-check
npm --prefix src/HybridWebView/samples/HybridWebApp/Web test
npm --prefix src/HybridWebView/samples/HybridWebApp/Web run stage
```

Run npm checks serially: SDK builds/staging share `dist` inputs. Do not globally
override `TargetFrameworks` across repository project references. Runtime public
API ledgers use the repository's generation/validation targets.

Copy the **whole `src/HybridWebView` product tree**, excluding ignored outputs,
when testing source portability. The source-free `tests/UnitTests/PackageConsumer`
instead references the runtime NuGet and its bundled analyzer, with reflection
serialization disabled. Both paths require an explicitly selected, compatible
MAUI 11 SDK/package version and feeds. Native execution, native AOT and platform
certification must be validated separately; portable tests do not certify them.
