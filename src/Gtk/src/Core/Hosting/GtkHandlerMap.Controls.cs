using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Gtk.Hosting
{
	public static partial class GtkHandlerMap
	{
		// The remaining Core handlers (progress, range, pickers, input, graphics, containers and menus). Called from the static constructor
		// through AddCoreEntries.
		static void AddControlEntries()
		{
			Add<ActivityIndicatorHandler, GtkActivityIndicatorHandler>();
			Add<ProgressBarHandler, GtkProgressBarHandler>();
			Add<SliderHandler, GtkSliderHandler>();
			Add<StepperHandler, GtkStepperHandler>();
			Add<PickerHandler, GtkPickerHandler>();
			Add<DatePickerHandler, GtkDatePickerHandler>();
			Add<TimePickerHandler, GtkTimePickerHandler>();
			Add<RadioButtonHandler, GtkRadioButtonHandler>();
			Add<SearchBarHandler, GtkSearchBarHandler>();
			Add<ImageButtonHandler, GtkImageButtonHandler>();
			Add<ShapeViewHandler, GtkShapeViewHandler>();
			Add<GraphicsViewHandler, GtkGraphicsViewHandler>();
			Add<IndicatorViewHandler, GtkIndicatorViewHandler>();
			Add<RefreshViewHandler, GtkRefreshViewHandler>();
			Add<SwipeViewHandler, GtkSwipeViewHandler>();
			Add<SwipeItemMenuItemHandler, GtkSwipeItemMenuItemHandler>();
			Add<SwipeItemViewHandler, GtkSwipeItemViewHandler>();
			Add<MenuBarHandler, GtkMenuBarHandler>();
			Add<MenuBarItemHandler, GtkMenuBarItemHandler>();
			Add<MenuFlyoutHandler, GtkMenuFlyoutHandler>();
			Add<MenuFlyoutSubItemHandler, GtkMenuFlyoutSubItemHandler>();
			Add<MenuFlyoutSeparatorHandler, GtkMenuFlyoutSeparatorHandler>();
			Add<MenuFlyoutItemHandler, GtkMenuFlyoutItemHandler>();

			AddDefault(typeof(IActivityIndicator), typeof(GtkActivityIndicatorHandler));
			AddDefault(typeof(IProgress), typeof(GtkProgressBarHandler));
			AddDefault(typeof(ISlider), typeof(GtkSliderHandler));
			AddDefault(typeof(IStepper), typeof(GtkStepperHandler));
			AddDefault(typeof(IPicker), typeof(GtkPickerHandler));
			AddDefault(typeof(IDatePicker), typeof(GtkDatePickerHandler));
			AddDefault(typeof(ITimePicker), typeof(GtkTimePickerHandler));
			AddDefault(typeof(IRadioButton), typeof(GtkRadioButtonHandler));
			AddDefault(typeof(ISearchBar), typeof(GtkSearchBarHandler));
			AddDefault(typeof(IImageButton), typeof(GtkImageButtonHandler));
			AddDefault(typeof(IShapeView), typeof(GtkShapeViewHandler));
			AddDefault(typeof(IGraphicsView), typeof(GtkGraphicsViewHandler));
			AddDefault(typeof(IIndicatorView), typeof(GtkIndicatorViewHandler));
			AddDefault(typeof(IRefreshView), typeof(GtkRefreshViewHandler));
			AddDefault(typeof(ISwipeView), typeof(GtkSwipeViewHandler));
			AddDefault(typeof(ISwipeItemMenuItem), typeof(GtkSwipeItemMenuItemHandler));
			AddDefault(typeof(ISwipeItemView), typeof(GtkSwipeItemViewHandler));
			AddDefault(typeof(IMenuBar), typeof(GtkMenuBarHandler));
			AddDefault(typeof(IMenuBarItem), typeof(GtkMenuBarItemHandler));
			AddDefault(typeof(IMenuFlyout), typeof(GtkMenuFlyoutHandler));
			AddDefault(typeof(IMenuFlyoutSubItem), typeof(GtkMenuFlyoutSubItemHandler));
			AddDefault(typeof(IMenuFlyoutSeparator), typeof(GtkMenuFlyoutSeparatorHandler));
			AddDefault(typeof(IMenuFlyoutItem), typeof(GtkMenuFlyoutItemHandler));
		}
	}
}
