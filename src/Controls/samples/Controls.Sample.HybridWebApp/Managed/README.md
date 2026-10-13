# Maui.HybridApp — experimental managed product

This independently packable class library is a **WIP under the sample tree**,
not app code copied into an assembly and not an official framework package.
`Maui.HybridApp` / `0.1.0-preview.1` is an unverified working NuGet identity;
name availability, ownership and publication authorization have not been checked.
Local `dotnet pack` is permitted. Registry publication is a separate maintainer
decision and has not occurred.

## Product and host boundary

The library owns `CommandRegistry`, the bounded per-WebView session hub,
`IHybridAppTarget.Invoke(string)`, protocol/control metadata and four
independently registered Essentials providers. It references Essentials and uses
the standard `Microsoft.Extensions.DependencyInjection.Abstractions` API supplied by .NET 11,
never Core/Controls, sample identity, UI, development forwarding or test doubles.
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
`TimeProvider.System` clock and **only the four currently implemented** product
providers: AppInfo, DeviceInfo, Clipboard and Preferences. It supplies their real
existing `IAppInfo`, `IDeviceInfo`, `IClipboard` and `IPreferences` defaults with
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
letters, digits, underscores or hyphens. No entire-store clear is exposed.

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
The framework's native-context scopes are not a reason to assume this overridden
page construction automatically receives a matching scope.

`MainPage(HybridAppTargetObject)` contains only view integration, not registry or
provider factories. It subscribes to the hub's typed `NativeEventReceived` event,
marshals to the UI thread and rechecks both its stopped state and current document
before sending. `Stop` detaches host event/view handlers and Development resources;
the owning scope disposes the hub. Handler replacement disconnects its document
without disposing the still-live scope. Frontend reload keeps scoped providers and
counter state; another window gets independent instances.

Hub disposal is idempotent: it cancels work, retires the document, disposes its
`TimeProvider` lease timer and clears all native-event listeners. There is no page
callback factory, page/hub DI cycle or `BuildServiceProvider` call in app startup.

## External providers

A Toolkit assembly implements `IHybridCommandProvider.Register(registry)` or
calls `registry.Register<TArgs,TResult>(name, inputInfo, outputInfo, handler)`.
The host opts in with `AddHybridProvider<TProvider>()` after `AddHybridApp`, or
explicit registry composition in a custom native host. The generic hub itself
does not choose Essentials/sample providers. Providers own their DTOs, generated contexts and
topics (`RegisterTopic`). No scanning, attributes, reflection router, catalog
or edits to the protocol context are needed. Duplicate names/topics are rejected.
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
# In the MAUI checkout: a UI-free target for tests and local package checks.
bash ../../../../eng/common/dotnet.sh test Tests/Playground.UnitTests.csproj \
  -p:TargetFrameworks=net11.0 -p:TargetFramework=net11.0
bash ../../../../eng/common/dotnet.sh pack Managed/Maui.HybridApp.csproj \
  -p:TargetFrameworks=net11.0
```

The full default target list includes net11.0 and the normal Android, iOS,
MacCatalyst and (on Windows) Windows TFMs, not Tizen. An in-tree build references
the actual Essentials project. App project references pass per-platform artifact
directories through to Essentials; no product code is source-linked.

After copying the bundle to another repository, the library's MSBuild island
never imports ancestor MAUI/sample configuration. Supply a real, compatible
`Microsoft.Maui.Essentials` 11 package version and its feed:

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

The NuGet contains the product DLL/XML per packed TFM, README and MIT license
(also a portable PDB when the in-tree build includes it).
Essentials and its genuine transitive dependencies are normal NuGet dependencies.
The product uses DI abstractions from the .NET 11 framework reference/runtime,
not a redundant PackageReference: such a reference causes NU1510 in a copied
standalone project. No warning/analyzer/pruning suppression is used.
The full DI container is a test/consumer dependency pinned to the repository's
`11.0.0-rtm.26480.113`, not a product dependency or Core/Controls reference.
no app/sample/test source, UI assets or runtime fakes are shipped. The separate
`Tests/PackageConsumer` executable references the **package**, uses plugin-owned
generated JSON and disables reflection serialization. It exercises the actual
one-call registration, scoped target/providers, events and independent scopes.
Before release, inspect
the nuspec/file list, run assembly and packed-consumer tests, validate the native
platform matrix and review naming/support/signing requirements. Do not publish
this working identity without a separate authorization.

To run that consumer, supply a dedicated `NuGet.Config` whose cleared sources
contain the product package and **real** Essentials/Graphics/DI dependencies:

```sh
dotnet restore Tests/PackageConsumer/PackageConsumer.csproj --configfile NuGet.Config
dotnet run --project Tests/PackageConsumer/PackageConsumer.csproj --no-restore
```

For an isolated proof, copy only `Tests/PackageConsumer`, the `Plugin*.cs` fixture
files from `Tests/ExternalProvider`, and `Tests/Directory.Build.props/targets` to
a fresh directory with a copy-local SDK pin/config. No `Managed` directory or
product project reference is needed. The normal unit tests instead reference the
real product project and a separate external provider project.
