# .NET MAUI Playground

<!-- impeccable:product-schema 1 -->

## Platform

web

The Svelte interface runs inside a native .NET MAUI HybridWebView. A standalone
browser can explore the interface but does not emulate native capabilities.

## Users

Developers exploring a web-first .NET MAUI application and its implemented
JavaScript/native APIs. Feature code must be easy to find and change without
understanding the entire application.

Web developers must be able to compose and control the app from TypeScript.
They must not write C# menu definitions or sample-specific native methods to
use normal product capabilities.

## Product Purpose

Show real native capabilities through a small, approachable playground.
The Welcome group introduces the experience; its second page offers hands-on
native demonstrations. Detailed feature pages remain directly accessible.

## Operating Context

The frontend uses Svelte 5, TypeScript and Vite. Native heads target .NET 11 on
Android, iOS, Mac Catalyst and Windows. Vite supports live frontend editing;
explicit staging packages the same interface for offline execution.

The WIP product libraries and the sample live in one movable sample bundle.
The managed and JavaScript products are separate from sample commands and UI.
Registry publication and migration to MAUI Labs are separate decisions.

## Capabilities and Constraints

- Native actions use explicitly registered, typed commands, not browser mocks.
- Public TypeScript APIs create, configure, update and dispose native resources.
  Native menu descriptions, actions, navigation, window creation, browser panes
  and dialog interactions are authored in the web application.
- C# implements platform behavior, safe transport, validation and host lifetime.
  It must not choose the sample's menu structure, pages or application actions.
- Product APIs expose the supported MAUI operations and options in idiomatic
  TypeScript, using Tauri as a usability reference. They are not fixed demo
  commands or narrowly predetermined sample workflows.
- Modules expose capability functions directly: picking files, showing dialogs,
  reading or changing the application theme and acquiring window handles do not
  require an application/service factory or a public native-client argument.
- One shared native connection belongs to each web document. Resource handles
  retain their native identity and owner privately; different windows do not
  share a global document client.
- Window, menu-item and other native-resource operations use explicit asynchronous
  getters/setters. `currentWindow()` itself is a cheap synchronous local handle;
  the native operation is what callers await.
- Picker options are selected by TypeScript. Picking a file and reading it are
  separate operations; large and binary files are not rejected merely because
  the sample previews a bounded amount of text.
- Transport chunks and resource/lifetime budgets protect the implementation.
  Sample preview limits must not become product-wide file-size/type limits.
- Interactive resources expose focused getters, setters and actions rather than
  a broad information object as their primary API.
- The root JavaScript API owns infrastructure; feature modules own their APIs.
- Native providers are composed through DI and use an owning WebView scope.
- Each feature has a readable Svelte component.
- Navigation groups replace the category dropdown and may be collapsed.
- The interface is a playground, not an implementation-tracking dashboard.
  Unimplemented experiments and priorities do not appear in normal navigation.
- The document and header stay fixed; navigation and content scroll independently.
- Header controls collapse the feature navigation and session pane. Frontend
  reload lives in the diagnostics pane and reloads only the current document.
  Session diagnostics, progress and logs
  belong to the document, not a feature page; cancellation remains in the header.
- Desktop session diagnostics can be resized with a pointer or keyboard separator.
  At compact widths, both panes are full-height flyouts rather than persistent
  miniature strips. Only one flyout opens at a time, with Escape, outside-click,
  focus containment and focus return. Native browser guests hide behind overlays.
- Window basics observes the current native window's real size events; position
  events and centering are capability-gated Windows operations. Multiple windows
  separately owns trusted URL/ID/title/options and caller-owned child resources.
- A remote website must not receive the playground's native command bridge.
- The next feature batch includes native windows, dialogs, file picking,
  device readouts and a browser tab/pane inside the web app's own layout.
  The approved desktop proof anchors an ordinary native browser guest to a
  measured DOM slot in BrowserPanel.svelte. It is not a DOM element or iframe;
  separate windows, modal pages and a sibling native-grid region are not used.
  Framing policy is not bypassed. Geometry, clipping, focus, visibility and
  native/DOM layering limits must remain explicit; mobile guests are unsupported.
  Desktop native menus are created and updated through TypeScript, and native
  item activation invokes TypeScript callbacks. The sample's menu recipes and
  actions live in the web frontend, not C#.
  Real OS deep-link and app-action
  activation are deferred; opening an app window at a route is not an OS deep link.

## Brand Commitments

Keep the .NET MAUI Playground name and the current restrained visual identity.
This is a feature-focused playground, not a fictional themed app or a marketing
site. Do not copy community application branding, catalogs or assets.

## Product Principles

- Demonstrate real behavior and make unsupported, denied or cancelled outcomes clear.
- Reach a useful native interaction quickly without mandatory onboarding.
- Keep product APIs, sample behavior and hosting glue distinct.
- Preserve user intent and native state across frontend navigation and reload.
- State preview and platform limitations instead of implying universal support.

## Evidence on Hand

`../README.md` records implementation and validation scope. `../FEATURES.md` is
the implementation backlog. Product API documentation and generated-reference
instructions live in `../JavaScript/` and `../Managed/`.
No customer testimonials, adoption claims or stable-product certification are
supplied or required.
