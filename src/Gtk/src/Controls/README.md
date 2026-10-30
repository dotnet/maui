# Microsoft.Maui.Controls.Gtk

GTK 4 handlers for `Microsoft.Maui.Controls`, on top of [`Microsoft.Maui.Gtk`](../Core) (Core backend) and
[`Microsoft.Maui.Essentials.Gtk`](../Essentials).

```csharp
MauiApp.CreateBuilder().UseMauiApp<App>().UseMauiGtk();   // Core + Controls + Essentials, the one call an app makes
```

`UseMauiGtk()` = `UseMauiGtkCore()` + `UseMauiGtkEssentials()` + the Controls registrations below. Mappers are per handler and sit in
front of the stock ones (see the Gtk README), so there is no ordering rule with `AppendToMapping`.

## Status

| Area | Handler | GTK 4 | Notes |
|---|---|---|---|
| NavigationPage | `GtkNavigationViewHandler` (Core) | `Gtk.Stack` of `MauiGtkFillHost` | slide transition when animated; only the top page is realised |
| Toolbar | `GtkControlsToolbarHandler` : `GtkToolbarHandler` (Core) | `Gtk.HeaderBar` as the window title bar | back button, drawer toggle, items (primary = buttons, secondary = overflow popover), title view/icon, colours |
| FlyoutPage | `GtkFlyoutViewHandler` (Core) | `MauiGtkFlyoutPanel` | `Split` = locked side by side; `Popover` = slide-over with scrim (click dismisses) |
| Label | `GtkControlsLabelHandler` : `GtkLabelHandler` | `Gtk.Label` | `FormattedText` = Pango `span`s, `TextType.Html` = Pango markup (small HTML subset), `TextTransform`, `LineBreakMode`, `MaxLines`; span gestures unsupported |
| DisplayAlert / ActionSheet / Prompt | `GtkDialogs` via the alert delegate convention (keyed `Func<Page, ..., Task<...>>` services) | modal `Gtk.Window` transient for the page's window | not `Gtk.AlertDialog`: its async GIO call cannot be driven or inspected; buttons are named after their text |
| TabbedPage | `GtkTabbedPageHandler` : `GtkTabbedViewHandler` (Core) | `MauiGtkTabView` (toggle-button bar + `Gtk.Stack`) | bar on top; icons; bar/selected/unselected colours |

Not done yet: see the section "Open issues" of the area reports; the table above only lists what is implemented and tested.

## Notes

- **Pages in containers.** Navigation, tabs and flyout hold each page in a `MauiGtkFillHost`: from inside its size-allocate it measures the
  page with the exact allocation and arranges it to fill (measure before allocate, no deferral).
- **Title bar owner.** The window, a flyout page and a modal root page can each own a toolbar; the title bar belongs to whoever set it last.
- **Window activation.** Pages `Appear` when the virtual window activates. A bare X server (no window manager) never activates a toplevel;
  `GtkWindowHandler.UpdateActive(true)` (internal, used by the tests) runs the lifecycle in that case.

## Tests

`src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Controls/` (xUnit, real widgets, `ControlsWindowHost` hosts a page in a `Gtk.Window`).
