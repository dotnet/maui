# Toolbar drawer toggle contract

`Microsoft.Maui.IToolbarDrawerToggleVisible.DrawerToggleVisible` tells a platform backend whether the
toolbar should render the drawer (flyout / "hamburger") affordance in its navigation slot.

## Why a separate interface

The member is **not** on `IToolbar`. `IToolbar` is a shipped public interface, and adding a member to it
would source-break every existing external implementer on `netstandard2.0`, where default interface
members are not supported and the member would therefore be abstract (`CS0535`).

`IToolbarDrawerToggleVisible` is an optional capability interface instead, so it is purely additive on
every target framework. This mirrors `ISwipeItemMenuItemIconColor`, which was added alongside
`ISwipeItemMenuItem` for the same reason.

Consume it by pattern matching:

```csharp
bool drawerToggleVisible = toolbar is IToolbarDrawerToggleVisible { DrawerToggleVisible: true };
```

A toolbar that does not implement the interface has no drawer toggle.

## Ownership

The value is **computed and owned by the cross-platform layer**, which is why the contract is read-only:

| Toolbar | How the value is computed |
| ------- | ------------------------- |
| `ShellToolbar` | `FlyoutBehavior == Flyout` and either the navigation stack has a single page or the back button is suppressed by a `BackButtonBehavior`. On Windows only `FlyoutBehavior == Flyout` is considered. |
| `NavigationPageToolbar` | The toolbar's parent is a `FlyoutPage`, `FlyoutPage.ShouldShowToolbarButton()` is `true`, and (outside Windows) no page has been pushed. |

Platform backends **render** this value; they must not compute or overwrite it.
`Microsoft.Maui.Controls.Toolbar` still exposes a settable `DrawerToggleVisible` property because it is
shipped public API, but the framework owns the value for the Shell and `NavigationPage` toolbars.

## Back button precedence — not mutual exclusion

`BackButtonVisible` and `DrawerToggleVisible` are **not** mutually exclusive. On Windows, `ShellToolbar`
sets the drawer toggle purely from `FlyoutBehavior`, so both values can be `true` at the same time.

What is guaranteed is **precedence**: the two share a single navigation slot, and the back button wins.
A backend should check the back button first and only fall through to the drawer toggle, exactly as the
built-in `ToolbarExtensions.UpdateBackButton` implementations do on Android and Tizen.

Ordering is also guaranteed: the drawer toggle backing value is updated *before* `BackButtonVisible`
notifies, and the `DrawerToggleVisible` notification is raised *after* it. So a backend rendering the
shared slot from either mapper always observes a settled pair rather than a half-applied transition.
For the same reason, code that forwards toolbar state between instances must assign `BackButtonVisible`
before `DrawerToggleVisible` (see `ShellToolbarTracker.ApplyToolbarChanges`).

## Change notification

Changes raise a handler update keyed on `"DrawerToggleVisible"`, exactly like any other toolbar property:

```csharp
public class MyToolbarHandler : ElementHandler<IToolbar, MyPlatformToolbar>
{
    public static readonly IPropertyMapper<IToolbar, MyToolbarHandler> Mapper =
        new PropertyMapper<IToolbar, MyToolbarHandler>(ElementMapper)
        {
            [nameof(IToolbar.Title)] = MapTitle,
            [nameof(IToolbar.BackButtonVisible)] = MapNavigationSlot,
            [nameof(IToolbarDrawerToggleVisible.DrawerToggleVisible)] = MapNavigationSlot,
        };

    public MyToolbarHandler() : base(Mapper) { }

    protected override MyPlatformToolbar CreatePlatformElement() => new();

    static void MapTitle(MyToolbarHandler handler, IToolbar toolbar) =>
        handler.PlatformView.Title = toolbar.Title;

    static void MapNavigationSlot(MyToolbarHandler handler, IToolbar toolbar)
    {
        if (toolbar.BackButtonVisible)                                          // back wins
            handler.PlatformView.ShowBackButton();
        else if (toolbar is IToolbarDrawerToggleVisible { DrawerToggleVisible: true })
            handler.PlatformView.ShowDrawerToggle();
        else
            handler.PlatformView.ClearNavigationSlot();
    }
}
```

Pointing both keys at one method is the recommended shape, since the two properties render into the same
slot.

Each window owns its own toolbar instance, so drawer toggle state and its notifications are tracked per
window with no additional work from the backend.

## Built-in platform consumption

| Platform | Consumes `DrawerToggleVisible`? | Where |
| -------- | ------------------------------- | ----- |
| Android | Yes | `Controls.Platform.ToolbarExtensions.UpdateBackButton` selects a `DrawerArrowDrawable` with `Progress = 0` when the back button is hidden and the drawer toggle is visible. Shell additionally drives an `ActionBarDrawerToggle` from `ShellToolbarTracker`. |
| Tizen | Yes | `Controls.Platform.ToolbarExtensions.UpdateBackButton` installs a menu button, and `UpdateTitleIcon` avoids clearing the icon while the drawer toggle is visible. |
| Windows | No | The flyout toggle is rendered by `NavigationView`, not by the toolbar. |
| iOS / MacCatalyst | No | The flyout toggle is owned by the flyout/navigation controller, not by the toolbar. |

The `DrawerToggleVisible` property mapping is therefore registered only for Android and Tizen. Because
that mapping and the `BackButtonVisible` mapping both call `UpdateBackButton`, a change that flips both
values runs it twice; this is intentional and idempotent.
