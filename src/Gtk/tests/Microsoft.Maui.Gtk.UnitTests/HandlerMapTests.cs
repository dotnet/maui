using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Gtk.Handlers;
using Xunit;

namespace Microsoft.Maui.Gtk.UnitTests
{
	/// <summary>The stock-to-GTK handler substitution for the Core handlers of this area.</summary>
	public class HandlerMapTests
	{
		public static TheoryData<Type, Type> CoreInterfaces => new()
		{
			{ typeof(IActivityIndicator), typeof(GtkActivityIndicatorHandler) },
			{ typeof(IProgress), typeof(GtkProgressBarHandler) },
			{ typeof(ISlider), typeof(GtkSliderHandler) },
			{ typeof(IStepper), typeof(GtkStepperHandler) },
			{ typeof(IPicker), typeof(GtkPickerHandler) },
			{ typeof(IDatePicker), typeof(GtkDatePickerHandler) },
			{ typeof(ITimePicker), typeof(GtkTimePickerHandler) },
			{ typeof(IRadioButton), typeof(GtkRadioButtonHandler) },
			{ typeof(ISearchBar), typeof(GtkSearchBarHandler) },
			{ typeof(IImageButton), typeof(GtkImageButtonHandler) },
			{ typeof(IShapeView), typeof(GtkShapeViewHandler) },
			{ typeof(IGraphicsView), typeof(GtkGraphicsViewHandler) },
			{ typeof(IIndicatorView), typeof(GtkIndicatorViewHandler) },
			{ typeof(IRefreshView), typeof(GtkRefreshViewHandler) },
			{ typeof(ISwipeView), typeof(GtkSwipeViewHandler) },
			{ typeof(ISwipeItemMenuItem), typeof(GtkSwipeItemMenuItemHandler) },
			{ typeof(ISwipeItemView), typeof(GtkSwipeItemViewHandler) },
			{ typeof(IMenuBar), typeof(GtkMenuBarHandler) },
			{ typeof(IMenuBarItem), typeof(GtkMenuBarItemHandler) },
			{ typeof(IMenuFlyout), typeof(GtkMenuFlyoutHandler) },
			{ typeof(IMenuFlyoutSubItem), typeof(GtkMenuFlyoutSubItemHandler) },
			{ typeof(IMenuFlyoutSeparator), typeof(GtkMenuFlyoutSeparatorHandler) },
			{ typeof(IMenuFlyoutItem), typeof(GtkMenuFlyoutItemHandler) },
		};

		[GtkTheory]
		[MemberData(nameof(CoreInterfaces))]
		public void CoreOnlyAppsGetTheGtkHandlerOfEachInterface(Type virtualView, Type expected) => GtkTestHost.Run(() =>
		{
			var factory = GtkTestApp.App.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.Equal(expected, factory.GetHandlerType(virtualView));
		});

		public static TheoryData<Type, Type> ControlsViews => new()
		{
			{ typeof(ActivityIndicator), typeof(GtkActivityIndicatorHandler) },
			{ typeof(ProgressBar), typeof(GtkProgressBarHandler) },
			{ typeof(Slider), typeof(GtkSliderHandler) },
			{ typeof(Stepper), typeof(GtkStepperHandler) },
			{ typeof(Picker), typeof(GtkPickerHandler) },
			{ typeof(DatePicker), typeof(GtkDatePickerHandler) },
			{ typeof(TimePicker), typeof(GtkTimePickerHandler) },
			{ typeof(RadioButton), typeof(GtkRadioButtonHandler) },
			{ typeof(SearchBar), typeof(GtkSearchBarHandler) },
			{ typeof(ImageButton), typeof(GtkImageButtonHandler) },
			{ typeof(GraphicsView), typeof(GtkGraphicsViewHandler) },
			{ typeof(Ellipse), typeof(GtkShapeViewHandler) },
			{ typeof(IndicatorView), typeof(GtkIndicatorViewHandler) },
			{ typeof(RefreshView), typeof(GtkRefreshViewHandler) },
			{ typeof(SwipeView), typeof(GtkSwipeViewHandler) },
			{ typeof(SwipeItem), typeof(GtkSwipeItemMenuItemHandler) },
			{ typeof(MenuBar), typeof(GtkMenuBarHandler) },
			{ typeof(MenuBarItem), typeof(GtkMenuBarItemHandler) },
			{ typeof(MenuFlyout), typeof(GtkMenuFlyoutHandler) },
			{ typeof(MenuFlyoutSubItem), typeof(GtkMenuFlyoutSubItemHandler) },
			{ typeof(MenuFlyoutSeparator), typeof(GtkMenuFlyoutSeparatorHandler) },
			{ typeof(MenuFlyoutItem), typeof(GtkMenuFlyoutItemHandler) },
		};

		[GtkTheory]
		[MemberData(nameof(ControlsViews))]
		public void ControlsViewsGetTheGtkHandler(Type view, Type expected) => GtkTestHost.Run(() =>
		{
			var factory = GtkTestApp.App.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.Equal(expected, factory.GetHandlerType(view));
		});
	}
}
