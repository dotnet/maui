using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
		{
			handlersCollection.AddHandler<CollectionView, CollectionViewHandler>();
			handlersCollection.AddHandler<CarouselView, CarouselViewHandler>();
			handlersCollection.AddHandler<Application, ApplicationHandler>();
			handlersCollection.AddHandler<ActivityIndicator, ActivityIndicatorHandler>();
			handlersCollection.AddHandler<BoxView, ShapeViewHandler>();
			handlersCollection.AddHandler<Button, ButtonHandler>();
			handlersCollection.AddHandler<CheckBox, CheckBoxHandler>();
			handlersCollection.AddHandler<DatePicker, DatePickerHandler>();
			handlersCollection.AddHandler<Editor, EditorHandler>();
			handlersCollection.AddHandler<Entry, EntryHandler>();
			handlersCollection.AddHandler<GraphicsView, GraphicsViewHandler>();
			handlersCollection.AddHandler<Image, ImageHandler>();
			handlersCollection.AddHandler<Label, LabelHandler>();
			handlersCollection.AddHandler<Layout, LayoutHandler>();
			handlersCollection.AddHandler<Picker, PickerHandler>();
			handlersCollection.AddHandler<ProgressBar, ProgressBarHandler>();
			handlersCollection.AddHandler<ScrollView, ScrollViewHandler>();
			handlersCollection.AddHandler<SearchBar, SearchBarHandler>();
			handlersCollection.AddHandler<Slider, SliderHandler>();
			handlersCollection.AddHandler<Stepper, StepperHandler>();
			handlersCollection.AddHandler<Switch, SwitchHandler>();
			handlersCollection.AddHandler<TimePicker, TimePickerHandler>();
			handlersCollection.AddHandler<Page, PageHandler>();
			handlersCollection.AddHandler<WebView, WebViewHandler>();
			handlersCollection.AddHandler<Border, BorderHandler>();
			handlersCollection.AddHandler<IContentView, ContentViewHandler>();
			handlersCollection.AddHandler<Shapes.Ellipse, ShapeViewHandler>();
			handlersCollection.AddHandler<Shapes.Line, LineHandler>();
			handlersCollection.AddHandler<Shapes.Path, PathHandler>();
			handlersCollection.AddHandler<Shapes.Polygon, PolygonHandler>();
			handlersCollection.AddHandler<Shapes.Polyline, PolylineHandler>();
			handlersCollection.AddHandler<Shapes.Rectangle, RectangleHandler>();
			handlersCollection.AddHandler<Shapes.RoundRectangle, RoundRectangleHandler>();
			handlersCollection.AddHandler<Window, WindowHandler>();
			handlersCollection.AddHandler<ImageButton, ImageButtonHandler>();
			handlersCollection.AddHandler<IndicatorView, IndicatorViewHandler>();
			handlersCollection.AddHandler<RadioButton, RadioButtonHandler>();
			handlersCollection.AddHandler<RefreshView, RefreshViewHandler>();
			handlersCollection.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();
			handlersCollection.AddHandler<SwipeView, SwipeViewHandler>();

			handlersCollection.AddHandler<MenuBar, MenuBarHandler>();
			handlersCollection.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
			handlersCollection.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
			handlersCollection.AddHandler<MenuBarItem, MenuBarItemHandler>();

#if ANDROID || IOS
			handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
#endif
#if WINDOWS || ANDROID
			handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
			handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
			handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
			handlersCollection.AddHandler<TabbedPage,  TabbedViewHandler>();
#if WINDOWS
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
			handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
			handlersCollection.AddHandler<Shell, ShellHandler>();
#endif
#endif
			return handlersCollection;
		}

		internal static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder)
		{
			builder.ConfigureImageSources(services =>
			{
				services.AddService<FileImageSource>(svcs => new FileImageSourceService(svcs.CreateLogger<FileImageSourceService>()));
				services.AddService<FontImageSource>(svcs => new FontImageSourceService(svcs.GetRequiredService<IFontManager>(), svcs.CreateLogger<FontImageSourceService>()));
				services.AddService<StreamImageSource>(svcs => new StreamImageSourceService(svcs.CreateLogger<StreamImageSourceService>()));
				services.AddService<UriImageSource>(svcs => new UriImageSourceService(svcs.CreateLogger<UriImageSourceService>()));
			});

			return builder;
		}

		internal static MauiAppBuilder RemapForControls(this MauiAppBuilder builder)
		{
			// Update the mappings for IView/View to work specifically for Controls
			VisualElement.RemapForControls();
			Label.RemapForControls();
			Button.RemapForControls();
			RadioButton.RemapForControls();
			FlyoutPage.RemapForControls();
			Toolbar.RemapForControls();
			Window.RemapForControls();
			Editor.RemapForControls();
			Entry.RemapForControls();
			SearchBar.RemapForControls();
			TabbedPage.RemapForControls();
			Layout.RemapForControls();
			Shape.RemapForControls();

			return builder;
		}
	}
}