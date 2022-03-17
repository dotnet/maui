using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.Maui.Dispatching;


#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#elif WINDOWS
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
#elif IOS || MACCATALYST
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Handlers.Compatibility;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
			where TApp : class, IApplication
		{
#pragma warning disable RS0030 // Do not used banned APIs - don't want to use a factory method here
			builder.Services.TryAddSingleton<IApplication, TApp>();
#pragma warning restore RS0030
			builder.SetupDefaults();
			return builder;
		}

		public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
			where TApp : class, IApplication
		{
			builder.Services.TryAddSingleton<IApplication>(implementationFactory);
			builder.SetupDefaults();
			return builder;
		}

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

#if WINDOWS || ANDROID || IOS || MACCATALYST
			handlersCollection.AddHandler(typeof(ListView), typeof(Handlers.Compatibility.ListViewRenderer));
			handlersCollection.AddHandler(typeof(Cell), typeof(Handlers.Compatibility.CellRenderer));
			handlersCollection.AddHandler(typeof(ImageCell), typeof(Handlers.Compatibility.ImageCellRenderer));
			handlersCollection.AddHandler(typeof(EntryCell), typeof(Handlers.Compatibility.EntryCellRenderer));
			handlersCollection.AddHandler(typeof(TextCell), typeof(Handlers.Compatibility.TextCellRenderer));
			handlersCollection.AddHandler(typeof(ViewCell), typeof(Handlers.Compatibility.ViewCellRenderer));
			handlersCollection.AddHandler(typeof(SwitchCell), typeof(Handlers.Compatibility.SwitchCellRenderer));
			handlersCollection.AddHandler(typeof(TableView), typeof(Handlers.Compatibility.TableViewRenderer));
			handlersCollection.AddHandler(typeof(Frame), typeof(Handlers.Compatibility.FrameRenderer));
#endif

#if IOS || MACCATALYST
			handlersCollection.AddHandler(typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer));
			handlersCollection.AddHandler(typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer));
			handlersCollection.AddHandler(typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer));
#endif

#if ANDROID || IOS || MACCATALYST
			handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
			handlersCollection.AddHandler<Shell, ShellRenderer>();
#endif
#if WINDOWS || ANDROID
			handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
			handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
			handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
			handlersCollection.AddHandler<TabbedPage, TabbedViewHandler>();
#endif

#if WINDOWS
			handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
			handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
			handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
			handlersCollection.AddHandler<Shell, ShellHandler>();
#endif
			return handlersCollection;
		}

		static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
		{
#if WINDOWS || ANDROID || IOS || MACCATALYST
			// initialize compatibility DependencyService
			DependencyService.SetToInitialized();
			DependencyService.Register<Xaml.ResourcesLoader>();
			DependencyService.Register<Xaml.ValueConverterProvider>();
			DependencyService.Register<ResourcesProvider>();
			DependencyService.Register<PlatformSizeService>();
			DependencyService.Register<PlatformInvalidate>();
			DependencyService.Register<FontNamedSizeService>();
#endif

			builder.ConfigureImageSourceHandlers();
			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddMauiControlsHandlers();
				});

#if WINDOWS
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiControlsInitializer>());
#endif
			builder.RemapForControls();

			return builder;
		}

		class MauiControlsInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
#if WINDOWS
				var dispatcher =
					services.GetService<IDispatcher>() ??
					MauiWinUIApplication.Current.Services.GetRequiredService<IDispatcher>();

				dispatcher
					.DispatchIfRequired(() =>
					{
						var dictionaries = UI.Xaml.Application.Current?.Resources?.MergedDictionaries;
						if (dictionaries != null)
						{
							// Microsoft.Maui.Controls
							UI.Xaml.Application.Current?.Resources?.AddLibraryResources("MicrosoftMauiControlsIncluded", "ms-appx:///Microsoft.Maui.Controls/Platform/Windows/Styles/Resources.xbf");
						}
					});
#endif
			}
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