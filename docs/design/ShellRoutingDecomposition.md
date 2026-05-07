# Shell Routing Decomposition

## Overview

Shell today conflates three responsibilities into a single class hierarchy: **visual composition** (tabs, flyout), **route registry** (mapping URIs to pages), and **navigation stack management** (push/pop/modal). This coupling means developers who want Shell's routing and deep-link support must also adopt Shell's visual structure — even when they need custom tab bars, flyouts, or page layouts that Shell cannot express.

This proposal separates Shell's **router** from its **compositor**, making route-based navigation available to any MAUI app regardless of whether it uses `AppShell.xaml`.

### Motivation

Partner feedback from production apps (including apps dating back to Xamarin.Forms) consistently describes this pattern:

> "We don't care for structuring the app in AppShell — we can create the structure within individual pages. What we could use is route-based navigation to ContentPages with deep links, simplified parameter passing, and hierarchical navigation."

Specific partner requirements that Shell cannot satisfy today:

- Custom center button in bottom tabs
- In-page tabs positioned below dynamic-height content
- Text wrapping in tab headers (accessibility)
- Horizontally scrolling dynamic tab headers
- Fully custom flyout section content

These teams have already built custom compositors. They want Shell's **routing engine** without Shell's **visual opinions**.

### Evidence from the Codebase

Source-code audit confirms the coupling:

- `Shell.cs` (~2300 lines): flyout management, route resolution, back button handling, navigation dispatch, query parameter flow, modal management — all in one class
- `ShellSection.cs`: simultaneously navigation stack manager, tab switcher, page container, and modal stack owner
- `ShellUriHandler.SearchPath()`: route resolution walks the visual tree, making routes inseparable from the tab/flyout hierarchy
- Two separate route registries: XAML routes (visual tree `Route` properties) vs `Routing.RegisterRoute` (static dictionary) — merged at query time via `GlobalRouteItem` wrappers

### Related Issues

| Issue | Title | Relevance |
|-------|-------|-----------|
| [#30195](https://github.com/dotnet/maui/issues/30195) | [WiP] The next Shellvolution | Parent epic |
| [#35107](https://github.com/dotnet/maui/issues/35107) | Shell GoToAsync: no way to pass query parameters to intermediate pages | Route templates solve this |
| [#5312](https://github.com/dotnet/maui/issues/5312) | Shell Routing through attributes on page (source generators) | `[Route]` attribute |
| [#3917](https://github.com/dotnet/maui/issues/3917) | Give developers full control via RouteFactory for all views | DI for tab roots |
| [#3868](https://github.com/dotnet/maui/issues/3868) | Shell should support query parameters on ShellContent in XAML | Route templates solve this |
| [#9649](https://github.com/dotnet/maui/issues/9649) | Separate IQueryAttributable into netstandard project | VM testability |
| [#20902](https://github.com/dotnet/maui/issues/20902) | Add more descriptive exception for route not found | NavigationResult |
| [#27589](https://github.com/dotnet/maui/issues/27589) | GoToAsync should use Shell's Dispatcher | Auto UI-thread dispatch |
| [#13537](https://github.com/dotnet/maui/issues/13537) | Tabs in AppShell.xaml don't invoke IQueryAttributable on VMs | INavigationAware |
| [#31266](https://github.com/dotnet/maui/issues/31266) | [NET11] Rework OnBackButtonPressed | IConfirmNavigation |
| [#11307](https://github.com/dotnet/maui/issues/11307) | "Pending Navigations still processing" exception | Navigation queueing |
| [#17608](https://github.com/dotnet/maui/issues/17608) | "Pending Navigations still processing" on Windows | Navigation queueing |
| [#6193](https://github.com/dotnet/maui/issues/6193) | Fix GoToAsync so task resolves after navigation finishes | NavigationResult |
| [#12162](https://github.com/dotnet/maui/issues/12162) | PresentationMode="Modal" doesn't put Page into ModalStack | Modal improvements |
| [#32985](https://github.com/dotnet/maui/issues/32985) | Shell Handlers for Android | Complementary platform work |
| [#21816](https://github.com/dotnet/maui/issues/21816) | [Spec, WiP] Better hooks for Shell services and scopes | DI/service scope hooks |
| [#30180](https://github.com/dotnet/maui/issues/30180) | Proposal for Enhanced Customization of App Shell | "Blank canvas" Shell |

---

## Architecture

### Current State

```
┌─────────────────────────────────────────────────┐
│                    Shell                         │
│                                                  │
│  Visual Compositor   Route Registry   Nav Stack  │
│  (tabs, flyout,      (XAML routes +   (push/pop, │
│   toolbar, title)     RegisterRoute)   modals)   │
│                                                  │
│  All three are tightly coupled in Shell.cs,      │
│  ShellSection.cs, and ShellUriHandler.cs         │
└─────────────────────────────────────────────────┘
```

### Target State

```
┌───────────────────┐  ┌───────────────────────────────────┐
│   Shell            │  │   Shell Router                     │
│   (OPTIONAL)       │  │   (ALWAYS available)               │
│                    │  │                                    │
│   Tab/Flyout UI    │  │   Route registry                  │
│   Toolbar          │  │   URI → Page resolution           │
│   Title view       │  │   Route templates ({param})       │
│                    │  │   Deep link support                │
│   Uses Shell Router│  │   Parameter passing                │
│   internally       │  │   Navigation stack management     │
│                    │  │   INavigationService               │
└───────────────────┘  └───────────────────────────────────┘
     Use if you want        Register routes in code,
     Shell's built-in       navigate from ViewModels,
     tab/flyout UI          get deep links — no XAML needed
```

---

## Proposed API

### 1. `INavigationService` — Testable Navigation Abstraction

ViewModels should be able to navigate without referencing `Shell.Current`. This interface lives in `Microsoft.Maui` (not `Microsoft.Maui.Controls`) so ViewModels in netstandard projects can use it.

```csharp
namespace Microsoft.Maui.Navigation;

public interface INavigationService
{
    Task<NavigationResult> GoToAsync(string route);
    Task<NavigationResult> GoToAsync(string route, IDictionary<string, object> parameters);
    Task<NavigationResult> GoToAsync(string route, bool animate);
    Task<NavigationResult> GoToAsync(string route, bool animate,
        IDictionary<string, object> parameters);
    Task<NavigationResult> GoBackAsync();
    Task<NavigationResult> GoBackAsync(IDictionary<string, object> parameters);

    string CurrentRoute { get; }
}
```

**Implementation** delegates to Shell internally:

```csharp
namespace Microsoft.Maui.Controls;

internal sealed class ShellNavigationService : INavigationService
{
    public Task<NavigationResult> GoToAsync(string route) =>
        Shell.Current.TryGoToAsync(new ShellNavigationState(route));

    public Task<NavigationResult> GoBackAsync() => GoToAsync("..");

    public string CurrentRoute =>
        Shell.Current?.CurrentState?.Location?.ToString() ?? "";
}
```

**ViewModel usage** — no Shell reference needed:

```csharp
public class ProductsViewModel
{
    private readonly INavigationService _nav;

    public ProductsViewModel(INavigationService nav) => _nav = nav;

    public async Task OpenProduct(string sku)
    {
        var result = await _nav.GoToAsync($"product/{sku}");
        if (!result.IsSuccessful)
            Debug.WriteLine($"Navigation failed: {result.FailureReason}");
    }
}
```

**Unit testing** with a fake:

```csharp
public sealed class FakeNavigationService : INavigationService
{
    public List<string> NavigatedRoutes { get; } = new();
    public string CurrentRoute { get; set; } = "";

    public Task<NavigationResult> GoToAsync(string route)
    {
        NavigatedRoutes.Add(route);
        CurrentRoute = route;
        return Task.FromResult(NavigationResult.Success());
    }

    // ... other overloads
}

[Test]
public async Task OpenProduct_NavigatesToCorrectRoute()
{
    var nav = new FakeNavigationService();
    var vm = new ProductsViewModel(nav);

    await vm.OpenProduct("seed-tomato");

    Assert.AreEqual("product/seed-tomato", nav.NavigatedRoutes.Single());
}
```

### 2. `NavigationResult` + `TryGoToAsync()` — Non-Throwing Navigation

Shell currently swallows exceptions silently (confirmed at `Shell.cs:1411` and `Shell.cs:1707` where `catch` blocks log warnings but never re-throw). `TryGoToAsync` makes failure explicit.

```csharp
namespace Microsoft.Maui.Controls;

public sealed class NavigationResult
{
    public bool IsSuccessful => Status == NavigationStatus.Succeeded;
    public NavigationStatus Status { get; }
    public string? FailureReason { get; }
    public Exception? Exception { get; }

    public static NavigationResult Success() =>
        new(NavigationStatus.Succeeded);
    public static NavigationResult RouteNotFound(string route) =>
        new(NavigationStatus.RouteNotFound,
            $"No route registered for '{route}'. " +
            "Register with Routing.RegisterRoute() or declare in Shell XAML.");
    public static NavigationResult Cancelled() =>
        new(NavigationStatus.Cancelled);
    public static NavigationResult Failed(Exception ex) =>
        new(NavigationStatus.Failed, ex.Message, ex);
}

public enum NavigationStatus
{
    Succeeded,
    RouteNotFound,
    Cancelled,
    NavigationInProgress,
    Failed
}
```

New overloads on `Shell`:

```csharp
public partial class Shell
{
    // Existing — unchanged
    public Task GoToAsync(ShellNavigationState state);

    // New — non-throwing
    public Task<NavigationResult> TryGoToAsync(ShellNavigationState state);
    public Task<NavigationResult> TryGoToAsync(ShellNavigationState state,
        bool animate);
    public Task<NavigationResult> TryGoToAsync(ShellNavigationState state,
        IDictionary<string, object> parameters);
    public Task<NavigationResult> TryGoToAsync(ShellNavigationState state,
        bool animate, IDictionary<string, object> parameters);
}
```

### 3. Route Templates with Path Parameters

Aligns Shell routing with ASP.NET Core and Blazor. Routes can contain `{param}` placeholders that match URI segments and extract values. This is additive — existing literal routes are unchanged.

**Registration:**

```csharp
// Literal (existing — unchanged)
Routing.RegisterRoute("products", typeof(ProductsPage));

// Templated (new)
Routing.RegisterRoute("product/{sku}", typeof(ProductDetailPage));
Routing.RegisterRoute("review", typeof(ReviewPage));
Routing.RegisterRoute("order/{orderId}", typeof(OrderDetailPage));
```

**Navigation:**

```csharp
// Parameter is in the path, not query string
await Shell.Current.GoToAsync("//main/products/product/seed-tomato");
// ProductDetailPage receives: sku = "seed-tomato"

// Multi-level push — both pages get sku
await Shell.Current.GoToAsync("//main/products/product/seed-tomato/review");
// ProductDetailPage receives: sku = "seed-tomato"
// ReviewPage inherits: sku = "seed-tomato" (from parent path)
```

**Receiving parameters** — existing mechanisms work unchanged:

```csharp
// Option 1: IQueryAttributable (existing)
public class ProductDetailPage : ContentPage, IQueryAttributable
{
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var sku = (string)query["sku"];
    }
}

// Option 2: [QueryProperty] (existing)
[QueryProperty(nameof(Sku), "sku")]
public partial class ProductDetailPage : ContentPage
{
    public string Sku { get; set; }
}
```

**Route matching rules:**

1. Literal segments match first: `product/featured` beats `product/{sku}` for URI `product/featured`
2. Template segments match when no literal matches: `product/{sku}` matches `product/seed-tomato`
3. A template can consume multiple URI segments: `product/{sku}` consumes both `product` and `seed-tomato`
4. Path parameters mix with query strings: `product/seed-tomato?highlight=true` delivers `sku` from path and `highlight` from query

**XAML compatibility:** `{param}` syntax does not conflict with XAML markup extensions — XAML only interprets `{` as markup when it's the very first character of the attribute value. `Route="product/{sku}"` is a literal string.

**Implementation scope** (4 files):

| File | Change |
|------|--------|
| `Routing.cs` | Parse `{param}` from route string at registration time |
| `ShellUriHandler.cs` | Template-aware matching in `GetItems()` / `SearchPath()` |
| `RouteRequestBuilder.cs` | Track extracted path params per matched segment |
| `ShellNavigationManager.cs` | Merge path params into `ShellRouteParameters` |

### 4. `INavigationAware` — ViewModel Lifecycle Hooks

Shell currently does not call `IQueryAttributable` on ViewModels during tab switches ([#13537](https://github.com/dotnet/maui/issues/13537)). `OnBackButtonPressed` is inconsistent across platforms ([#31266](https://github.com/dotnet/maui/issues/31266)). These interfaces provide consistent lifecycle hooks on ViewModels.

```csharp
namespace Microsoft.Maui.Navigation;

/// <summary>
/// Implement on ViewModels to receive navigation lifecycle callbacks.
/// Shell checks both the Page and its BindingContext.
/// </summary>
public interface INavigationAware
{
    Task OnNavigatedToAsync(NavigationContext context, CancellationToken ct);
    Task OnNavigatingFromAsync(NavigationContext context, CancellationToken ct);
}

/// <summary>
/// Implement to gate whether navigation away is allowed.
/// Replaces OnBackButtonPressed for Shell pages.
/// </summary>
public interface IConfirmNavigation
{
    Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken ct);
}

public sealed class NavigationContext
{
    public NavigationMode Mode { get; }  // Push, Pop, PopToRoot, TabSwitch, Modal
    public IReadOnlyDictionary<string, object> Parameters { get; }
    public string SourceRoute { get; }
    public string TargetRoute { get; }
}

public enum NavigationMode
{
    Push,
    Pop,
    PopToRoot,
    TabSwitch,
    Modal,
    Unknown
}
```

**ViewModel usage:**

```csharp
public class CheckoutViewModel : ObservableObject, INavigationAware, IConfirmNavigation
{
    private bool _hasUnsavedItems;

    public async Task<bool> CanNavigateAsync(NavigationContext context, CancellationToken ct)
    {
        if (!_hasUnsavedItems) return true;
        return await Shell.Current.DisplayAlert(
            "Abandon Cart?", "You have items in your cart.", "Leave", "Stay");
    }

    public Task OnNavigatedToAsync(NavigationContext context, CancellationToken ct)
    {
        // Called for both GoToAsync AND tab switches
        return LoadCartAsync(ct);
    }

    public Task OnNavigatingFromAsync(NavigationContext context, CancellationToken ct)
        => Task.CompletedTask;
}
```

### 5. Auto UI-Thread Dispatch

All `GoToAsync` and `TryGoToAsync` calls auto-dispatch to the UI thread. This is an internal behavior fix, not new API surface. Fixes [#27589](https://github.com/dotnet/maui/issues/27589).

```csharp
// Internal implementation — not public API
public partial class Shell
{
    public Task<NavigationResult> TryGoToAsync(ShellNavigationState state)
    {
        if (!Dispatcher.IsDispatchRequired)
            return TryGoToAsyncCore(state);

        return Dispatcher.DispatchAsync(() => TryGoToAsyncCore(state));
    }
}
```

### 6. Navigation Queueing

Replace the `"Pending Navigations still processing"` crash ([#11307](https://github.com/dotnet/maui/issues/11307), [#17608](https://github.com/dotnet/maui/issues/17608)) with bounded queueing:

```csharp
// In ShellSection.cs — replace the throw with a queue
// BEFORE (current):
if (_handlerBasedNavigationCompletionSource != null)
    throw new InvalidOperationException("Pending Navigations still processing");

// AFTER:
if (_handlerBasedNavigationCompletionSource != null)
{
    if (_navigationQueue.Count >= MaxQueuedNavigations) // 5
        return NavigationResult.Failed(new InvalidOperationException(
            "Navigation queue full."));
    _navigationQueue.Enqueue(request);
    return;
}
```

### 7. `IQueryAttributable` Assembly Move

Move `IQueryAttributable` from `Microsoft.Maui.Controls` to `Microsoft.Maui` with type-forwarding for backward compatibility. Fixes [#9649](https://github.com/dotnet/maui/issues/9649).

```csharp
// New location: src/Core/src/Navigation/IQueryAttributable.cs
namespace Microsoft.Maui.Navigation;

public interface IQueryAttributable
{
    void ApplyQueryAttributes(IDictionary<string, object> query);
}

// Backward compat: type-forward from Controls
[assembly: TypeForwardedTo(typeof(Microsoft.Maui.Navigation.IQueryAttributable))]
```

---

## Phased Delivery

### Phase 1 — Foundation (Preview)

Smallest changes, highest value. Each is independently useful.

| Feature | Issues Fixed |
|---------|-------------|
| `NavigationResult` + `TryGoToAsync()` | #20902, #11307, #17608, #6193 |
| Navigation queueing | #11307, #17608 |
| `INavigationService` abstraction | #9649 |
| `IQueryAttributable` → `Microsoft.Maui` | #9649 |
| Auto UI-thread dispatch | #27589 |

### Phase 2 — Navigation Lifecycle (Preview)

| Feature | Issues Fixed |
|---------|-------------|
| `INavigationAware` / `IConfirmNavigation` | #13537, #31266, #7351 |
| Route templates `{param}` | #35107, #3868, #5312 |
| Fix dot-prefix params for DI pages | #35107 |

### Phase 3 — Polish (GA)

| Feature | Issues Fixed |
|---------|-------------|
| `INavigationGuard` (auth guards) | — |
| `[Route]` attribute + source generator | #5312 |
| `RouteFactory` on `ShellContent` | #3917 |

### Deferred (Future)

| Feature | Reason |
|---------|--------|
| Strongly-typed `GoToAsync<TViewModel>(TArgs)` | Requires VM-to-route mapping registry |
| Shell compositor/navigator full separation | Architectural refactor, builds on Phase 1-3 APIs |
| `MapTab()` / `MapModal()` code-first composition | Needs code-first Shell alternative |
| Full navigation middleware pipeline | Guards cover 90% of use cases |

---

## Backward Compatibility

All proposed changes are **additive**. Existing apps require zero changes:

- `AppShell.xaml` apps with XAML routes → compile and run unchanged
- `Routing.RegisterRoute(string, Type)` → works identically
- `GoToAsync(string)` → works identically (no behavior change)
- `IQueryAttributable` → type-forwarded, no source breaks
- `[QueryProperty]` → unchanged
- Route templates only activate when route string contains `{`

The only behavior change is UI-thread auto-dispatch on `GoToAsync`, which is opt-out if needed.

---

## Alternatives Considered

### ViewModel-First Routing (Codex 5.3 proposal)

Navigate by ViewModel type: `nav.GoToAsync<CustomerDetailViewModel>(new CustomerDetailArgs(id))`. Strong for testability but requires abandoning `AppShell.xaml` entirely — too aggressive for migration. Deferred to future work as opt-in on top of `INavigationService`.

### Full Navigation Middleware Pipeline (Sonnet 4.6 proposal)

ASP.NET-style `INavigationMiddleware` with `next()` semantics. Clean concept but adds ordering complexity and state combinations. `INavigationGuard` (evaluate-all-then-decide) covers 90% of use cases with a simpler mental model.

### Blazor Router Alignment

Blazor's model is URL-first/deterministic. MAUI's is imperative/lifecycle-first. Forcing alignment creates paradigm mismatch. Route templates (`{param}`) are the safe intersection point — same syntax, different execution model.

### Do Nothing

The bugs in #11307, #17608, #20902, #13537, #6193 are real and affecting partners. Not an option.
