# Toolbar drawer toggle contract

`Microsoft.Maui.IToolbar.DrawerToggleVisible` tells a platform backend whether the toolbar should render
the drawer (flyout / "hamburger") affordance in its navigation slot.

## Ownership

The value is **computed and owned by the cross-platform layer**, which is why the contract is read-only:

| Toolbar | How the value is computed |
| ------- | ------------------------- |
| `ShellToolbar` | `FlyoutBehavior == Flyout` and either the navigation stack has a single page or the back button is suppressed by a `BackButtonBehavior`. On Windows only `FlyoutBehavior == Flyout` is considered. |
| `NavigationPageToolbar` | The toolbar's parent is a `FlyoutPage`, `FlyoutPage.ShouldShowToolbarButton()` is `true`, and (outside Windows) no page has been pushed. |
| `Toolbar` (base) | Whatever a caller assigns to the settable `Microsoft.Maui.Controls.Toolbar.DrawerToggleVisible` property. |

`IToolbar.DrawerToggleVisible` has a default interface implementation returning `false`, so existing
`IToolbar` implementations stay source and binary compatible.

## Back button precedence

`BackButtonVisible` wins. When the back button is visible the navigation slot renders the back affordance
even if `DrawerToggleVisible` is `true`. Built-in backends encode this by checking `BackButtonVisible`
first and only falling through to the drawer icon (see `ToolbarExtensions.UpdateBackButton` on Android and
Tizen).

The framework also guarantees the ordering of the two notifications: the drawer toggle backing value is
updated *before* `BackButtonVisible` notifies, and the `DrawerToggleVisible` notification is raised
*after*. A backend that renders the shared navigation slot from either mapper therefore never observes a
"back button and drawer toggle are both visible" state.

## Change notification

Changes raise a handler update keyed on `"DrawerToggleVisible"`, exactly like any other toolbar property,
so an external backend maps it the same way it maps `Title` or `BackButtonVisible`:

```csharp
public class MyToolbarHandler : ElementHandler<IToolbar, MyPlatformToolbar>
{
    public static readonly IPropertyMapper<IToolbar, MyToolbarHandler> Mapper =
        new PropertyMapper<IToolbar, MyToolbarHandler>(ElementMapper)
        {
            [nameof(IToolbar.Title)] = MapTitle,
            [nameof(IToolbar.BackButtonVisible)] = MapNavigationSlot,
            [nameof(IToolbar.DrawerToggleVisible)] = MapNavigationSlot,
        };

    public MyToolbarHandler() : base(Mapper) { }

    protected override MyPlatformToolbar CreatePlatformElement() => new();

    static void MapTitle(MyToolbarHandler handler, IToolbar toolbar) =>
        handler.PlatformView.Title = toolbar.Title;

    static void MapNavigationSlot(MyToolbarHandler handler, IToolbar toolbar)
    {
        if (toolbar.BackButtonVisible)
            handler.PlatformView.ShowBackButton();
        else if (toolbar.DrawerToggleVisible)
            handler.PlatformView.ShowDrawerToggle();
        else
            handler.PlatformView.ClearNavigationSlot();
    }
}
```

Each window owns its own toolbar instance, so drawer toggle state and its notifications are tracked
per window with no additional work from the backend.
