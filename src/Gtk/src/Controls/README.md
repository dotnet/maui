# Microsoft.Maui.Controls.Gtk

GTK 4 handlers for `Microsoft.Maui.Controls`, on top of [`Microsoft.Maui.Gtk`](../Core) (Core backend) and
[`Microsoft.Maui.Essentials.Gtk`](../Essentials).

```csharp
MauiApp.CreateBuilder().UseMauiApp<App>().UseMauiGtk();   // Core + Controls + Essentials, the one call an app makes
```

`UseMauiGtk()` = `UseMauiGtkCore()` + `UseMauiGtkEssentials()` + the Controls registrations below. Mappers are per handler and sit in
front of the stock ones (see the Gtk README), so stock-mapper customisations still run, but always after the GTK implementation.

## Status

| Area | Handler | GTK 4 | Notes |
|---|---|---|---|
| NavigationPage | `GtkNavigationViewHandler` (Core) | `Gtk.Stack` of `MauiGtkFillHost` | slide transition when animated; only the top page is realised |
| Toolbar | `GtkControlsToolbarHandler` : `GtkToolbarHandler` (Core) | `Gtk.HeaderBar` as the window title bar | back button, drawer toggle, items (primary = buttons, secondary = overflow popover), title view/icon, colours |
| FlyoutPage | `GtkFlyoutViewHandler` (Core) | `MauiGtkFlyoutPanel` | `Split` = locked side by side; `Popover` = slide-over with scrim (click dismisses) |
| TabbedPage | `GtkTabbedPageHandler` : `GtkTabbedViewHandler` (Core) | `MauiGtkTabView` (toggle-button bar + `Gtk.Stack`) | bar on top; icons; bar/selected/unselected colours |
| Shell | `GtkShellHandler` | `MauiGtkFlyoutPanel`: flyout = `Gtk.ListBox`, detail = `MauiGtkTabView` | flyout items (titles), current item, a tab per section (bar hidden for one), pushed pages, behaviour/width/presented, shell toolbar. **Not yet**: flyout icons/header/footer/templates, tabs for several contents of one section, search, appearance |
| CollectionView | `GtkCollectionViewHandler` | `Gtk.ListView` / `Gtk.GridView` over a C# `GListModel` (`GtkItemsSource`) | native virtualisation (GTK keeps about 200 rows realised), template + selector, linear/grid layouts, item spacing, header/footer (above/below the scroller), empty view, none/single/multiple selection with `Selected` visual state, groups (header/footer rows, linear only), `ScrollTo`, scroll bar visibility. **Not**: reorder, sizing strategy, scroll events, `RemainingItemsThreshold` |
| CarouselView | `GtkCarouselViewHandler` | the same list, pages as wide as the viewport | snaps 120 ms after scrolling stops; `Position`/`CurrentItem` both ways; peek insets; swipe on/off. **Not**: `Loop`, bounce |
| ListView, TableView | `GtkListViewHandler`, `GtkTableViewHandler` | `Gtk.ListView` / scrolled column | cells as views (view/text/image/switch/entry), selection, `ItemTapped`, header/footer, grouping (header row), separators. **Not**: context actions, pull to refresh |
| Label | `GtkControlsLabelHandler` : `GtkLabelHandler` | `Gtk.Label` | `FormattedText` = Pango `span`s, `TextType.Html` = Pango markup (small HTML subset), `TextTransform`, `LineBreakMode`, `MaxLines`; span gestures unsupported |
| Shapes | `GtkLineHandler`, `GtkPathHandler`, `GtkPolygonHandler`, `GtkPolylineHandler`, `GtkRectangleHandler`, `GtkRoundRectangleHandler`, `GtkBoxViewHandler` : `GtkShapeViewHandler` (Core) | `MauiGtkGraphicsView` + `ShapeDrawable` | registered by view type (the stock handlers take no command mapper); their own properties (`X1..Y2`, `Data`, `Points`, `FillRule`, `RadiusX/Y`, `CornerRadius`, `Color`) repaint |
| Frame | `GtkFrameHandler` | `MauiGtkBorder` | rounded-rectangle shape of `CornerRadius` (unset = 5), 1 px `BorderColor` stroke, `Padding`; `HasShadow` = CSS `box-shadow` (an explicit `Shadow` uses the container shadow) |
| DisplayAlert / ActionSheet / Prompt | `GtkDialogs` via the alert delegate convention (keyed `Func<Page, ..., Task<...>>` services) | modal `Gtk.Window` transient for the page's window | not `Gtk.AlertDialog`: its async GIO call cannot be driven or inspected; buttons are named after their text |
| Gestures | `GtkGestureConnection` (through `GesturePlatformBackend.Factory`) | `GestureClick` (tap, double tap, pointer press), `GestureDrag` (pan, swipe), `GestureZoom` (pinch), `EventControllerMotion` (pointer), `DragSource`/`DropTarget` (text) | controllers exist only for the recognizer kinds a view has; none claims a sequence. Drag and drop carries text; an in-process drop gets the source's whole `DataPackage` |
| Modal pages | `GtkModalNavigationBackend` (through `ModalNavigationBackend.Current`) | modal `Gtk.Window` transient for the page below, in its own window scope | the modal page's toolbar is that window's title bar; closing the window pops the page; `animated` is ignored |

## Notes

- **Seams in `Controls.Core`.** The platform-less build had no-op gesture and modal managers. Two small internal hooks make them
  pluggable: `GesturePlatformBackend.Factory` (`GesturePlatformManager.Standard.cs`) and `ModalNavigationBackend.Current`
  (`ModalNavigationManager.Standard.cs`). With nothing assigned the behaviour is unchanged (`ModalNavigationBackendTests` runs the no-backend Standard path through `ModalNavigationManager.PlatformModalStack`). Tests: `GesturePlatformBackendTests`,
  `ModalNavigationBackendTests` in `Controls.Core.UnitTests`.
- **Pages in containers.** Navigation, tabs, flyout, Shell and modal windows hold each page in a `MauiGtkFillHost`: from inside its
  size-allocate it measures the page with the exact allocation and arranges it to fill (measure before allocate, no deferral). List rows
  use `MauiGtkItemHost`, which asks for its content's size instead.
- **Title bar owner.** The window, a flyout page, a Shell and a modal root page can each own a toolbar; the title bar belongs to whoever
  set it last, and an owner without a toolbar clears the bar only if it set it.
- **Window activation.** Pages `Appear` when the virtual window activates. A bare X server (no window manager) never activates a toplevel;
  `GtkWindowHandler.UpdateActive(true)` (internal, used by the tests) runs the lifecycle in that case.
- **Stale rows.** GTK leaves the widget of a removed row bound until it recycles it; `GtkItemsController` releases the template views of
  rows that left the model (`GtkRow.Removed`), so they do not stay in the items view's logical tree.
- **GTK warnings in tests.** Synthetic signal emissions (the gesture tests) make GTK log `gdk_event_get_event_type` criticals;
  `ControlsWindowHost` clears the recorded complaints when it is created.

## Tests

`src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Controls/` (xUnit, real widgets; `ControlsWindowHost` hosts a page in a `Gtk.Window`,
`ControlsTestApp` builds an app with `UseMauiGtk()`): Navigation (16), Alerts and Labels (35), Gestures (20), Modal (6), CollectionView
(21), Carousel (7), Legacy lists (7), Shell (6), Shapes and Frame (10).
