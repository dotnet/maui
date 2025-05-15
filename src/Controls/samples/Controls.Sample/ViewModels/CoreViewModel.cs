using System.Collections.Generic;
using Maui.Controls.Sample.Models;
using Maui.Controls.Sample.Pages;
using Maui.Controls.Sample.ViewModels.Base;

namespace Maui.Controls.Sample.ViewModels
{
	public class CoreViewModel : BaseGalleryViewModel
	{
		protected override IEnumerable<SectionModel> CreateItems() => new[]
		{
			new SectionModel(typeof(AlertsPage), "Alerts",
				"Displaying an alert, asking a user to make a choice, or displaying a prompt."),

			new SectionModel(typeof(ApplicationControlPage), "App Control",
				"Demonstrates the app control features."),

			new SectionModel(typeof(AppThemeBindingPage), "AppThemeBindings",
				"Devices typically include light and dark themes, which each refer to a broad set of appearance preferences that can be set at the operating system level. Applications should respect these system themes, and respond immediately when the system theme changes."),

			new SectionModel(typeof(BrushesPage), "Brushes",
				"A brush enables you to paint an area, such as the background of a control, using different approaches."),

			new SectionModel(typeof(ClipPage), "Clip",
				"Defines the outline of the contents of an element."),

			new SectionModel(typeof(ContextFlyoutPage), "ContextFlyout",
				"Right-click context menu for controls."),

			new SectionModel(typeof(ContentPageGallery), "ContentPage",
				"Demonstrates using a Content Page."),

			new SectionModel(typeof(DevicePage), "Device",
				"A number of properties and methods to help developers customize layout and functionality on a per-platform basis"),

			new SectionModel(typeof(DispatcherPage), "Dispatcher",
				"Managing UI thread access with dispatchers and timers."),

			new SectionModel(typeof(EffectsPage), "Effects",
				"Apply Effects to a View."),

			new SectionModel(typeof(FlyoutPageGallery), "FlyoutPage",
				"Demonstrates using a Flyout Page."),

			new SectionModel(typeof(FocusPage), "Focus Management",
				"Focus and onfocus views, detect when a view gains focus and more."),

			new SectionModel(typeof(GesturesPage), "Gestures",
				"Use tap, pinch, pan, swipe, drag and drop, and pointer gestures on View instances."),

			new SectionModel(typeof(InputTransparentPage), "InputTransparent",
				"Manage whether a view participates in the user interaction cycle."),

			new SectionModel(typeof(MenuBarPage), "MenuBar",
				"Menu Bar is a horizontal bar that shows menu items."),

			new SectionModel(typeof(ModalPage), "Modal",
				"Allows you to push and pop Modal Pages."),

			new SectionModel(typeof(MultiWindowPage), "Multi-Window",
				"Allows you to open a new Window in the App."),

			new SectionModel(typeof(NavigationGallery), "Navigation Page",
				"Play with the different Navigation APIs."),

			new SectionModel(typeof(SemanticsPage), "Semantics",
				".NET MAUI allows accessibility values to be set on user interface elements by using Semantics values."),

			new SectionModel(typeof(ShadowPage), "Shadows",
 				"Shadow is one way a user perceives elevation. Light above an elevated object creates a shadow on the surface below. The higher the object, the larger and softer the shadow becomes."),

			new SectionModel(typeof(ToolbarPage), "Toolbar",
				"Toolbar items are buttons that are typically displayed in the navigation bar."),

			new SectionModel(typeof(TransformationsPage), "Transformations",
				"Apply scale transformations, rotation, etc. to a View."),

			new SectionModel(typeof(WindowTitleBar), "Window Title Bar",
				"Window Title Bar."),
		};
	}
}
