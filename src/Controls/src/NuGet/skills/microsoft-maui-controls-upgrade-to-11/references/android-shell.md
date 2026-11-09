# Android Shell renderer customizations

Starting with MAUI 11 Preview 6, the documented Android Shell default is the
handler-based architecture.
The legacy `ShellRenderer` remains manually registerable; its presence in the
assembly does not mean it is the default. A version-only retarget that keeps a
custom renderer registration does not migrate that customization to the new
architecture.

## Extension points

For the published `11.0.0-rc.1.26451.6` package, the exact source shows:

- `Microsoft.Maui.Controls.Handlers.ShellHandler` is the Android default handler.
  Its native view is `MauiDrawerLayout`, not `ShellFlyoutRenderer`.
- `ShellHandler` has a parameterless constructor. A renderer constructor that
  accepts an Android `Context` is not the corresponding handler constructor.
- `ShellHandler` implements `IShellContext` and retains the protected virtual
  `CreateTrackerForToolbar(AndroidX.AppCompat.Widget.Toolbar)` hook.
- That hook returns `IShellToolbarTracker`. The existing `ShellToolbarTracker`
  constructor accepts `IShellContext`, the toolbar and a `DrawerLayout`; a
  tracker customization need not be rewritten simply because its owner changes.
- `PlatformView` supplies the handler's drawer layout. Inspect this API on the
  selected package rather than retaining a renderer-only native field.

Start with a subclass of the selected handler and port the relevant existing
override. Register the custom handler through `ConfigureMauiHandlers`/
`AddHandler` for the intended Shell type, after `UseMauiApp`. Remove the old
renderer registration; do not simultaneously register two implementations and
rely on ordering to mask an incomplete port.

Keep existing tracker behavior and call base implementations where appropriate.
If the app subscribes to Shell/page/native events, pair those subscriptions with
the owning object's cleanup. Test repeated navigation to detect stale state,
duplicate commands or accumulating subscriptions. Preserve the built-in back
handling rather than replacing it with an ad-hoc click callback.

This is an extension-point guide, not a universal rewrite for every renderer.
Custom item/section renderers, flyout internals or native hierarchy assumptions
need their own inspection. Verify registration and native ancestry at runtime,
not only that the custom class compiles.

## Version evidence

The RC1 package identifies source revision
`484132f9e51f4d1eae72dba038575d6102639730`:

- [Android ShellHandler](https://github.com/dotnet/maui/blob/484132f9e51f4d1eae72dba038575d6102639730/src/Controls/src/Core/Handlers/Shell/ShellHandler.Android.cs)
- [Handler constructor](https://github.com/dotnet/maui/blob/484132f9e51f4d1eae72dba038575d6102639730/src/Controls/src/Core/Handlers/Shell/ShellHandler.cs)
- [Default registration](https://github.com/dotnet/maui/blob/484132f9e51f4d1eae72dba038575d6102639730/src/Controls/src/Core/Hosting/AppHostBuilderExtensions.cs)
- [Toolbar tracker and disposal](https://github.com/dotnet/maui/blob/484132f9e51f4d1eae72dba038575d6102639730/src/Controls/src/Core/Compatibility/Handlers/Shell/Android/ShellToolbarTracker.cs)
- [Public MAUI 11 Shell change](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-11#android-shell-handler)

Reconcile these APIs with the user's selected package before applying them to a
later release. This reference does not establish Apple or Windows migration
coverage.
