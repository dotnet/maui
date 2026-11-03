# Shell Route Templates — Spec & Prototype

> Status: **Draft / Design Spike**
> Tracking issue: [dotnet/maui#35107](https://github.com/dotnet/maui/issues/35107)
> Related comments: [Proposal A](https://github.com/dotnet/maui/issues/35107#issuecomment-4306338706), [XAML compatibility](https://github.com/dotnet/maui/issues/35107#issuecomment-4306367201)
> Prototype: [`prototype/ShellRouteTemplates/`](../../prototype/ShellRouteTemplates) (28 passing xUnit tests)

This document specifies an **additive, opt-in** extension to `Microsoft.Maui.Controls` Shell routing
that allows route registrations to declare inline path parameters using the standard
`{param}` template syntax used by ASP.NET Core and Blazor.

```csharp
// New (opt-in)
Routing.RegisterRoute("product/{sku}", typeof(ProductDetailPage));
await Shell.Current.GoToAsync("//main/products/product/seed-tomato/review");
// → ProductDetailPage.Sku = "seed-tomato"
// → ProductReviewPage.Sku = "seed-tomato"  (inherited from parent template)
```

---

## 1. Goals & non-goals

### Goals

1. Enable a single `GoToAsync` call to push a multi-page navigation stack where
   intermediate pages receive their own parameters.
2. Match ASP.NET Core / Blazor `{param}` syntax exactly so the muscle memory transfers.
3. **Preserve every existing route registration and navigation call unchanged**
   — purely additive change.
4. Reuse the existing `[QueryProperty]` and `IQueryAttributable` delivery pipeline.

### Non-goals (out of scope for v1)

- Optional parameters (`{param?}`) — listed in §10 future work.
- Catch-all parameters (`{*rest}`) — §10.
- Route constraints (`{id:int}`) — §10.
- Mixed literal+parameter segments (`product-{sku}`) — §10.
- Changing `Shell.CurrentState.Location` formatting beyond what's required to
  round-trip a templated URI back through `GoToAsync`.

---

## 2. Why path parameters

Today, parameters travel as query string entries. A URI may have **at most one** `?`,
so multi-segment navigation cannot deliver parameters to intermediate pages:

```csharp
// Today — broken:
GoToAsync("//main/products/product/review?sku=seed-tomato");
// product/review is matched as one nested global-route push, but the only
// `?` belongs to the leaf — no clean way to address the "product" page in the middle.

// Workaround — two sequential pushes (causes flicker, breaks deep linking):
await Shell.Current.GoToAsync("product?sku=seed-tomato");
await Shell.Current.GoToAsync("review");
```

Path parameters fix this structurally. The parameter sits **inside the path**, where it
belongs to the segment it follows, and is naturally inherited by anything below it.

---

## 3. Surface design

### 3.1 Registration

```csharp
// All four forms supported. Existing literal routes unchanged.
Routing.RegisterRoute("product",                     typeof(CatalogPage));      // literal
Routing.RegisterRoute("product/{sku}",               typeof(ProductDetailPage)); // template
Routing.RegisterRoute("review",                      typeof(ProductReviewPage)); // literal child
Routing.RegisterRoute("order/{orderId}",             typeof(OrderDetailPage));   // template
```

XAML:

```xml
<!-- Safe: '{' is not the first character of the attribute value, so no markup-extension parsing -->
<ShellContent Route="product/{sku}" ContentTemplate="{DataTemplate pages:ProductDetailPage}" />

<!-- Edge case: parameter as the very first segment requires the standard XAML `{}` escape -->
<ShellContent Route="{}{sku}" ContentTemplate="{DataTemplate pages:ItemPage}" />
```

### 3.2 Absolute navigation

```csharp
await Shell.Current.GoToAsync("//main/products/product/seed-tomato");
// Matches "product/{sku}" → ProductDetailPage with sku=seed-tomato

await Shell.Current.GoToAsync("//main/products/product/seed-tomato/review");
// Matches "product/{sku}" then "review" → 2-page stack, both pages see sku=seed-tomato

await Shell.Current.GoToAsync("//main/orders/order/ORD-00001");
// Matches "order/{orderId}" → OrderDetailPage with orderId=ORD-00001
```

### 3.3 Relative navigation

```csharp
// Currently on //main/products
await Shell.Current.GoToAsync("product/seed-tomato");
// → //main/products/product/seed-tomato

// Currently on //main/products/product/seed-tomato
await Shell.Current.GoToAsync("review");
// → //main/products/product/seed-tomato/review

await Shell.Current.GoToAsync("..");
// Pops the review page; product detail remains with its already-set sku.
```

### 3.4 Parameter delivery

No new attribute needed. `[QueryProperty]` already understands "named parameter":

```csharp
[QueryProperty(nameof(Sku), "sku")]
public partial class ProductDetailPage : ContentPage { /* receives sku from path */ }

[QueryProperty(nameof(Sku), "sku")]
public partial class ProductReviewPage : ContentPage { /* inherits sku from parent template */ }

// Or via IQueryAttributable
public partial class ProductDetailPage : ContentPage, IQueryAttributable
{
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("sku", out var sku)) { /* … */ }
    }
}
```

Path parameters and query string parameters merge into the same dictionary that already
flows through `ShellNavigationManager.ApplyQueryAttributes`.

### 3.5 Mixing path params and query strings

```csharp
Routing.RegisterRoute("product/{sku}", typeof(ProductDetailPage));
await Shell.Current.GoToAsync("//main/products/product/seed-tomato?highlight=true");
// sku = "seed-tomato"  (path)
// highlight = "true"   (query string, applied to the leaf page only, as today)
```

Path param wins on key conflict — see §6.3.

---

## 4. Route matching

### 4.1 Algorithm

For each URI segment position, find every registered template that matches starting there.
Pick the **most specific** one. Specificity score: literal segment = 2, parameter segment = 1.
This mirrors ASP.NET Core's route precedence.

```
Routes registered:
  "product"                   specificity = 2
  "product/{sku}"             specificity = 3
  "{anything}"                specificity = 1
  "product/seed-tomato"       specificity = 4
  "{a}/{b}"                   specificity = 2

URI "product"                       → "product"            (2 wins over 1)
URI "product/seed-tomato"           → "product/seed-tomato" (4 wins over 3, 2)
URI "product/banana"                → "product/{sku}"      (3 wins over 1)
URI "review"                        → "{anything}"         (only match)
```

### 4.2 Parameter inheritance ("innermost wins")

When multiple templates extract the same key during one navigation, the deeper one wins:

```
GoToAsync("//main/a/parent-id/b/child-id")
Routes: "a/{id}", "b/{id}"
→ id = "child-id"
```

This matches ASP.NET Core's nested-route-value behavior. In practice, real app
templates avoid name collisions (`sku` vs `orderId` vs `lineId`), so this is a
deterministic-tie-breaker rather than a frequent occurrence.

### 4.3 Where this hooks into the real Shell code

Concrete files (verified against current `main`):

| File | Today | Required change |
|---|---|---|
| `Routing.cs` | `s_routes : Dictionary<string, RouteFactory>` keyed by literal route. | Detect `{...}` segments in `RegisterRoute`. Store a parsed `RouteTemplate` next to the factory. |
| `Routing.cs` | `GetRouteKeys()` returns the literal route keys. | Continue returning the template strings as-is — `ShellUriHandler` distinguishes templates by the `{` in the key. |
| `Routing.cs` | `GetOrCreateContent(route)` looks up the factory by exact route key. | Look up by **template key** (e.g. `"product/{sku}"`), not by the user-supplied path slice. |
| `ShellUriHandler.cs` | `SearchPath` and `FindAndAddSegmentMatch` compare segments with `==`. | When a `routeKey` contains `{`, parse it as `RouteTemplate` and call `TryMatch` instead of literal compare. |
| `RouteRequestBuilder.cs` | Tracks `_globalRouteMatches` (route keys) and `_matchedSegments` (URI segments) separately. | Add `_pathParameters : Dictionary<string,string>`. When `AddGlobalRoute` is called for a templated key, also record the extracted params. |
| `ShellNavigationManager.cs` (`GoToAsync`) | Builds `parameters` from `state.FullLocation`'s query string only. | Also seed `parameters` with the path params extracted during URI matching, **before** calling `ApplyQueryAttributes`. |
| `ShellNavigationManager.ApplyQueryAttributes` | Already filters by route prefix and applies to each shell element / leaf page. | **Unchanged.** The new params just appear in the dict it already processes. |
| `ShellSection.GetOrCreateFromRoute` | Calls `Routing.GetOrCreateContent(route)` with the *raw* matched segment. | Pass the **template key** instead so `s_routes` lookup succeeds. |

The third bullet in `RouteRequestBuilder` is the only place truly new state appears.
The rest is "swap one comparison for another" or "pass a different string".

---

## 5. Backward compatibility

Compatibility statement: **any code that compiled and worked before this change continues
to compile and work, with byte-identical runtime behavior, unless the developer puts
`{` into a route string they pass to `Routing.RegisterRoute`.**

Why we believe this:

1. The current `Routing.ValidateRoute` accepts any string that does not start with the
   internal `IMPL_` prefix. `{` characters are not currently validated, but they are
   also not produced by any templating mechanism, so no shipping app has them.
2. `ShellUriHandler` segment matching is `string.Equals(..., Ordinal)`. A literal
   route `"product"` will continue to match the URI segment `"product"` because the
   new template-aware matcher only runs when the *route key* contains `{`.
3. `ApplyQueryAttributes`, `[QueryProperty]`, `IQueryAttributable`, and
   `ShellRouteParameters` are unchanged. Path parameters are merged into the same
   dictionary that flows through them today.
4. `Shell.CurrentState.Location` continues to be the user-supplied URI string. Path
   parameters appear in the path naturally — no synthetic query-string suffix.

A small backwards-compat risk: a user who today registers a route literally named
`"product/{sku}"` (treating `{sku}` as a literal substring) would see a behavior change.
We consider this acceptable because (a) such a route would be unreachable today via
any reasonable URI (URIs don't contain `{`), and (b) the issue tracker shows zero
reports of this pattern.

---

## 6. Edge cases

### 6.1 Encoding

URI segments captured into a parameter are **URL-decoded** before delivery
(`Uri.UnescapeDataString`). So `GoToAsync("//main/products/product/seed%20tomato")`
delivers `sku = "seed tomato"`. This matches what `WebUtils.UnpackParameters` does for
query strings today.

### 6.2 Empty segments

The URI splitter (`ShellUriHandler.RetrievePaths` and the prototype's `UriParser`) drops
empty segments. So `"product//seed-tomato"` collapses to `["product", "seed-tomato"]`
before matching. No special handling needed — but it does mean **a parameter cannot
be empty**. `Routing.RegisterRoute("product/{sku}", …)` then `GoToAsync("//main/products/product/")`
fails to match (no second segment to capture) instead of binding `sku=""`.

### 6.3 Path param vs query string with same name

Path wins. If `RegisterRoute("product/{sku}", …)` and the user calls
`GoToAsync("//main/products/product/seed-tomato?sku=ignored")`, the page receives
`sku = "seed-tomato"`. Implementation-wise: path params are added to
`ShellRouteParameters` first; `SetQueryStringParameters` already only adds keys that
aren't already present (verified in `ShellRouteParameters.SetQueryStringParameters`).

### 6.4 Two templates capturing the same name

Innermost wins (§4.2). In practice this is "nested params override outer params", which
mirrors ASP.NET Core. Pages that want the *outer* value can read it before navigation
completes (it's still in the URI) or use distinct names.

### 6.5 Back navigation

Back navigation (`GoToAsync("..")` or system back button) pops one page from the stack.
The remaining pages keep the parameter values they were created with — those values
were applied during the original push. **No re-application happens.** This is the same
behavior as today.

### 6.6 Modal pages

Modal pushes (`Shell.PresentationMode = Modal`) work identically. The matcher doesn't
care about modality; it just produces an ordered list of `(template, params)` matches
that `ShellSection.GoToAsync` then walks and pushes one-by-one (modal or not based on
the page's presentation mode), exactly as today.

### 6.7 `[QueryProperty]` mismatch

If `RegisterRoute("product/{sku}", typeof(P))` but `P` has no `[QueryProperty(..., "sku")]`
and doesn't implement `IQueryAttributable`, the parameter is **silently ignored** for
that page. Same as today's behavior for query string keys with no matching property.
Children that *do* declare `[QueryProperty(..., "sku")]` still receive it via inheritance.

### 6.8 Same template registered twice

Throws (existing `Routing.ValidateRoute` already throws on duplicate route keys; the
template string is the dictionary key, so duplicates are caught for free).

### 6.9 Two templates that match the same URI with the same specificity

Today's `GenerateRoutePaths` would already throw `"Ambiguous routes matched"`. The new
matcher should produce the same error. The prototype's `IsBetter` tie-breaks on length
to avoid the throw for `(literal, param)` vs `(param, literal)` cases that an app
author would expect to disambiguate.

### 6.10 `Shell.CurrentState.Location` round-trip

After navigating to `//main/products/product/seed-tomato/review`, `CurrentState.Location`
should report exactly that URI. This is naturally true because the matcher consumed the
URI segments without re-formatting; the URI used to navigate is the URI displayed.

---

## 7. Prototype

Located at `prototype/ShellRouteTemplates/`. **Standalone library** that demonstrates the
two pieces that don't yet exist in MAUI:

1. `RouteTemplate.Parse` — parses `"product/{sku}"` into segments.
2. `RouteTemplate.TryMatch` — matches the template against URI segments and extracts params.
3. `RouteTable.MatchPath` — walks a URI end-to-end, picks the most-specific template
   at each position, accumulates extracted parameters.

It does **not** modify Shell internals (see §8 for why). What it *does* prove:

| Scenario | Test | Result |
|---|---|---|
| Parse literal-only route | `Parses_LiteralOnlyRoute` | ✓ |
| Parse template with params | `Parses_SingleParameterRoute`, `Parses_NestedParameters` | ✓ |
| Reject invalid templates | `Rejects_DuplicateParameterNames`, `Rejects_EmptyParameter`, `Rejects_MixedSegment_v1` | ✓ |
| Specificity scoring | `Specificity_FavorsLiteralSegments` (theory, 3 cases) | ✓ |
| Match literal segment | `Matches_LiteralRoute_ExtractsNoParams` | ✓ |
| Match template + extract | `Matches_TemplateRoute_ExtractsParam` | ✓ |
| Reject mismatched literal | `DoesNotMatch_WrongLiteral` | ✓ |
| Reject too-short URI | `DoesNotMatch_NotEnoughSegments` | ✓ |
| Match at offset (after shell items) | `Matches_AtNonZeroOffset` | ✓ |
| URL-decode captured value | `DecodesPercentEncodedValues` | ✓ |
| Literal beats template | `LiteralBeatsTemplate_WhenBothMatch` | ✓ |
| Template wins when literal doesn't apply | `Template_Wins_WhenLiteralDoesNotApply` | ✓ |
| Longer literal beats short template | `LongerLiteralChain_WinsOverShorterTemplate` | ✓ |
| Garden sample: 2-page stack with shared sku | `ChainsTwoTemplates_AndExtractsParamsForBoth` | ✓ |
| Inheritance via merged dictionary | `BothPagesSeeSku_ViaShellLikeApplyQueryAttributes` | ✓ |
| `order/{orderId}` from Garden | `OrderId_FromGardenSample` | ✓ |
| Two distinct params from chained templates | `NestedParams_FromTwoSeparateTemplates` | ✓ |
| Same param in two templates → innermost wins | `ChildSameParamName_OverridesParent` | ✓ |
| Unmatched tail segment | `ReturnsNull_WhenAnyUriSegmentIsUnmatched` | ✓ |
| Path + query string coexist | `PathParamAndQueryString_Coexist` | ✓ |
| Path wins over query string for same key | `PathParamWins_OverQueryStringSameKey` | ✓ |
| Literal-only registration unchanged | `LiteralRouteOnly_BehavesExactlyLikeBefore` | ✓ |
| Routes without `{` never templated | `RouteWithoutBraces_NeverParsedAsTemplate` | ✓ |

Run with:

```bash
cd prototype/ShellRouteTemplates.Tests
dotnet test
# 28 passed, 0 failed
```

---

## 8. What's NOT in the prototype (and why)

Decisions made deliberately:

1. **No modification of `Routing.cs`, `ShellUriHandler.cs`, etc. in the real MAUI tree.**
   Shell's matcher is a 1000-line state machine that interleaves shell-element matching
   (Shell → Item → Section → Content) with global-route matching. Bolting templates
   onto that requires understanding (a) when `routeKey == segment` is checked and which
   of those checks should become `template.TryMatch`, (b) how `RouteRequestBuilder`'s
   parallel `_globalRouteMatches` / `_matchedSegments` lists must be augmented with
   extracted params, and (c) how `ShellSection.GetOrCreateFromRoute` must use the
   template key (not the user segment) when calling `Routing.GetOrCreateContent`. That
   work needs MAUI maintainer review (see §11). The prototype isolates the *new*
   algorithmic pieces so the diff to the real Shell is clear.
2. **No XAML build-task changes.** XAML compatibility is verified by reading — `{` is
   only special when it's the first attribute character. No code change required.
3. **No DI / `IServiceProvider` integration.** Page creation already uses
   `ActivatorUtilities.GetServiceOrCreateInstance`; the parameter dictionary flows
   through `ApplyQueryAttributes` after creation, so DI is orthogonal.

---

## 9. Issues / open questions

### 9.1 Resolved during this spike

- ✅ **XAML markup-extension collision** — confirmed false alarm. `{` is only parsed
  as a markup extension when it is the first character of the attribute value. The
  `{}` escape covers the `Route="{sku}"` edge case.
- ✅ **Same-name parameters in nested templates** — innermost wins, consistent with
  ASP.NET Core. Test `ChildSameParamName_OverridesParent` covers this.
- ✅ **Mixed segments** (`product-{sku}`) — deferred. Rejected with a clear error in v1.
- ✅ **Specificity tie-breaking** — literal beats parameter; longer match beats shorter
  on equal specificity. Mirrors ASP.NET Core.

### 9.2 Open — need MAUI maintainer input

| # | Question | My recommendation |
|---|---|---|
| Q1 | Should the template key be canonicalized on registration (e.g. lowercased)? | **No.** Existing routes are case-sensitive ordinal; preserve. |
| Q2 | Should ambiguous matches throw at registration time or navigation time? | **Navigation time**, matching today's `GenerateRoutePaths` behavior. Registration ordering shouldn't affect validation. |
| Q3 | Should `Shell.CurrentState.Location` for a templated push echo back the *template* or the *resolved URI*? | **Resolved URI.** That's what the user typed and what makes the URI shareable. |
| Q4 | Where should the parsed `RouteTemplate` live? Cached next to the `RouteFactory` in `s_routes`, or in a parallel dict? | **Next to the factory.** Avoids a second lookup. Suggest a small `RouteEntry` record. |
| Q5 | How do we expose path params in the diagnostic surface (`Shell.Navigated` event args, etc.)? | Probably the merged dict already in `ShellNavigatedEventArgs.Source`/`Current`. Worth a separate API review. |
| Q6 | Should we surface a typed accessor (e.g. `Shell.Current.GetRouteValue<T>("sku")`) or rely on `[QueryProperty]`? | **Rely on `[QueryProperty]` for v1.** A typed accessor is a follow-up. |
| Q7 | What's the right error message when a registered template has `{` syntax errors? | Mirror ASP.NET Core's: name the template, point to the bad segment. |

### 9.3 Failed experiments / things that didn't work

- ❌ **Initial attempt at "anchor on first literal segment then walk"** — produced
  ambiguous results when two templates shared a literal prefix
  (`product/{sku}` vs `product/{id}/edit`). Switched to "evaluate every template at
  every position, score by specificity" (current algorithm). All 28 tests pass.
- ❌ **Tried to make parameter capture take query-string precedence over path** —
  realized this contradicts the issue-tracker comments and ASP.NET Core convention.
  Reversed: path wins. Added `PathParamWins_OverQueryStringSameKey` test to lock it in.
- ⚠️ **xUnit + `Microsoft.NET.Test.Sdk` package downgrade error** under SDK 11 preview.
  Fixed by pinning the prototype to SDK 9.x via `prototype/global.json` and adding an
  empty-ish `prototype/Directory.Build.props` to neutralize the repo's Arcade SDK
  dependency. **Not a design issue** — purely a build-environment quirk.

---

## 10. Future work

- **Optional parameters** `{sku?}` — match URI with or without that segment present.
  Slightly tricky for the matcher because it has to consider both the "consumed" and
  "skipped" branches.
- **Catch-all** `{*rest}` — captures the remainder of the URI as a single string.
  Useful for fallback / 404 routes.
- **Constraints** `{id:int}`, `{sku:regex(…)}` — type/format validation at match time.
  Already a known ASP.NET Core pattern; would compose cleanly with the existing matcher.
- **Mixed segments** `product-{sku}` — supported by ASP.NET Core; requires a small
  parser per segment (split literal/parameter parts).
- **Default values** in registration — `RegisterRoute("product/{sku=default-sku}", …)`.
- **Typed parameter accessor** — `Shell.Current.GetRouteValue<int>("orderId")`.

---

## 11. What would need MAUI team involvement

- Code changes to `Routing.cs`, `ShellUriHandler.cs`, `RouteRequestBuilder.cs`,
  `ShellNavigationManager.cs`, `ShellSection.cs` per §4.3.
- API review for any new public surface (probably none, if we reuse `[QueryProperty]`).
- Trim/AOT analysis: route templates are stored as strings; reflection access for
  `[QueryProperty]` is unchanged. No new trimmer warnings expected.
- Decision on the open questions in §9.2.
- Doc updates for the navigation chapter.

---

## 12. Experiment log

| Date | What | Result |
|---|---|---|
| 2026-04-23 | Read issue #35107 (Proposal A and XAML compat comments) | Confirmed scope: additive, `{param}` syntax, reuse `[QueryProperty]`. |
| 2026-04-23 | Read `Routing.cs`, `ShellUriHandler.cs`, `ShellNavigationManager.cs`, `ShellRouteParameters.cs`, `RouteRequestBuilder.cs`, `ShellSection.cs`, `QueryPropertyAttribute.cs` on `main` | Mapped the exact files/functions that need changes (§4.3). Confirmed `ApplyQueryAttributes` already does the work — path params just need to land in the same dictionary. |
| 2026-04-23 | Built `RouteTemplate` parser + `RouteTable` matcher prototype | First pass: 22 tests passing. Anchor-on-first-literal algorithm produced ambiguity in nested-template cases. |
| 2026-04-23 | Switched to "evaluate every template at every position, score by specificity" | All 28 tests pass; no ambiguity in the documented scenarios. |
| 2026-04-23 | Added xUnit project under preview SDK 11 → package-downgrade errors | Pinned prototype to SDK 9 via `prototype/global.json`; neutralized repo Arcade requirement with `prototype/Directory.Build.props`. |
| 2026-04-23 | Verified Garden-sample URIs | `//main/products/product/seed-tomato`, `…/review`, `//main/orders/order/ORD-00001` all extract correctly. Both pages in the 2-page stack share `sku` via the merged dictionary, demonstrating inheritance. |
