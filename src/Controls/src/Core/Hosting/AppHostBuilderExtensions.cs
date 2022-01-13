using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
		{
#if __IOS__ || __ANDROID__
			handlersCollection.AddHandler<CollectionView, CollectionViewHandler>();
#endif

#if WINDOWS
			handlersCollection.AddHandler<CollectionView, CollectionViewHandler>();
#endif
#if WINDOWS || __ANDROID__
			handlersCollection.AddHandler<Shell, ShellHandler>();
#endif
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
			handlersCollection.AddHandler<Shapes.Line, ShapeViewHandler>();
			handlersCollection.AddHandler<Shapes.Path, ShapeViewHandler>();
			handlersCollection.AddHandler<Shapes.Polygon, ShapeViewHandler>();
			handlersCollection.AddHandler<Shapes.Polyline, ShapeViewHandler>();
			handlersCollection.AddHandler<Shapes.Rectangle, ShapeViewHandler>();
			handlersCollection.AddHandler<Shapes.RoundRectangle, ShapeViewHandler>();
			handlersCollection.AddHandler<Window, WindowHandler>();
			handlersCollection.AddHandler<ImageButton, ImageButtonHandler>();
			handlersCollection.AddHandler<IndicatorView, IndicatorViewHandler>();
			handlersCollection.AddHandler<RadioButton, RadioButtonHandler>();
#if __ANDROID__ || __IOS__
			handlersCollection.AddHandler<RefreshView, RefreshViewHandler>();
			
#endif
#if WINDOWS || ANDROID
			handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
			handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
			handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
#endif
#if ANDROID
			handlersCollection.AddHandler<TabbedPage, Controls.Handlers.TabbedPageHandler>();
			handlersCollection.AddHandler<SwipeView, SwipeViewHandler>();
			handlersCollection.AddHandler<SwipeItem, SwipeItemMenuItemHandler>();
			handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
#endif
			return handlersCollection;
		}

		internal static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder)
		{
			builder.ConfigureImageSources(services =>
			{
				services.AddService<FileImageSource>(svcs => new FileImageSourceService(svcs.GetService<IImageSourceServiceConfiguration>(), svcs.CreateLogger<FileImageSourceService>()));
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

			return builder;
		}
	}
}
