#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

#if __ANDROID__
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using Microsoft.Maui.Graphics.Native;
using FrameRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.FrameRenderer;
using LabelRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.LabelRenderer;
using ImageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ImageRenderer;
using ButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ButtonRenderer;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat.Platform.DefaultRenderer;
#elif WINDOWS
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Graphics.Win2D;
using BoxRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.BoxViewBorderRenderer;
using CellRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.TextCellRenderer;
using Deserializer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsSerializer;
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using StreamImagesourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.UWP.StreamImageSourceHandler;
using ImageLoaderSourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.UWP.UriImageSourceHandler;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.DefaultRenderer;
#elif __IOS__
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Graphics.Native;
using WebViewRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.WkWebViewRenderer;
using NavigationPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.NavigationRenderer;
using TabbedPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.TabbedRenderer;
using FlyoutPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.PhoneFlyoutPageRenderer;
using RadioButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder UseMauiApp<TApp>(this IAppHostBuilder builder)
			where TApp : class, IApplication
		{
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddSingleton<IApplication, TApp>();
			});

			builder.SetupDefaults();

			return builder;
		}

		public static IAppHostBuilder UseMauiApp<TApp>(this IAppHostBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
			where TApp : class, IApplication
		{
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddSingleton<IApplication>(implementationFactory);
			});

			builder.SetupDefaults();

			return builder;
		}

		static IAppHostBuilder SetupDefaults(this IAppHostBuilder builder)
		{
			builder.ConfigureLifecycleEvents(events =>
			{
#if __ANDROID__
				events.AddAndroid(android => android
					.OnApplicationCreating((app) =>
					{
						// This is the initial Init to set up any system services registered by
						// Forms.Init(). This happens in the Application's OnCreate - before
						// any UI has appeared.
						// This creates a dummy MauiContext that wraps the Application.

						var services = MauiApplication.Current.Services;
						var mauiContext = new MauiContext(services, app);
						var state = new ActivationState(mauiContext);
						Forms.Init(state, new InitializationOptions { Flags = InitializationFlags.SkipRenderers });

						GraphicsPlatform.RegisterGlobalService(NativeGraphicsService.Instance);
					})
					.OnCreate((activity, bundle) =>
					{
						// This is the Init that sets up the first context from the activity.
						// There is still no official MauiContext since that happens just after this.

						var services = MauiApplication.Current.Services;
						var mauiContext = new MauiContext(services, activity);
						var state = new ActivationState(mauiContext, bundle);
						Forms.Init(state, new InitializationOptions { Flags = InitializationFlags.SkipRenderers });
					})
					.OnPostCreate((activity, bundle) =>
					{
						// This is the final Init that ensures the Forms type is using the same
						// MauiContext that is part of the rest of the maui application.

						var windows = Application.Current?.Windows;
						if (windows?.Count > 0)
						{
							var window = windows[0];
							var mauiContext =
								window.Handler?.MauiContext ??
								window.Page?.Handler?.MauiContext;

							if (mauiContext != null)
							{
								var state = new ActivationState(mauiContext, bundle);
								Forms.Init(state);
							}
						}
					}));
#elif __IOS__
				events.AddiOS(iOS => iOS
					.WillFinishLaunching((app, options) =>
					{
						// This is the initial Init to set up any system services registered by
						// Forms.Init(). This happens before any UI has appeared.
						// This creates a dummy MauiContext.

						var services = MauiUIApplicationDelegate.Current.Services;
						var mauiContext = new MauiContext(services);
						var state = new ActivationState(mauiContext);
						Forms.Init(state, new InitializationOptions { Flags = InitializationFlags.SkipRenderers });
						return true;
					})
					.FinishedLaunching((app, options) =>
					{
						// This is the final Init that ensures the Forms type is using the same
						// MauiContext that is part of the rest of the maui application.

						var windows = Application.Current?.Windows;
						if (windows?.Count > 0)
						{
							var window = windows[0];
							var mauiContext =
								window.Handler?.MauiContext ??
								window.Page?.Handler?.MauiContext;

							if (mauiContext != null)
							{
								var state = new ActivationState(mauiContext);
								Forms.Init(state);
							}
						}

						GraphicsPlatform.RegisterGlobalService(NativeGraphicsService.Instance);

						return true;
					}));
#elif WINDOWS
				events.AddWindows(windows => windows
					.OnLaunching((app, args) =>
					{
						// This is the initial Init to set up any system services registered by
						// Forms.Init(). This happens before any UI has appeared.
						// This creates a dummy MauiContext.
						// We need to call this so the Window and Root Page can new up successfully
						// The dispatcher that's inside of Forms.Init needs to be setup before the initial
						// window and root page start creating.
						// Inside OnLaunched we grab the MauiContext that's on the window so we can have the correct
						// MauiContext inside Forms

						var services = MauiWinUIApplication.Current.Services;
						var mauiContext = new MauiContext(services);
						var state = new ActivationState(mauiContext, args);
						Forms.Init(state, new InitializationOptions() { Flags = InitializationFlags.SkipRenderers });
						// TODO: Implement GetPathBounds in Microsoft.Maui.Graphics
						// GraphicsPlatform.RegisterGlobalService(W2DGraphicsService.Instance);
					})
					.OnLaunched((app, args) =>
					{
						// This is the final Init that ensures the Forms type is using the same
						// MauiContext that is part of the rest of the maui application.

						var windows = Application.Current?.Windows;
						if (windows?.Count > 0)
						{
							var window = windows[0];
							var mauiContext =
								window.Handler?.MauiContext ??
								window.Page?.Handler?.MauiContext;

							if (mauiContext != null)
							{
								var state = new ActivationState(mauiContext, args);
								Forms.Init(state);
							}
						}
					}));
#endif
			});

			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddMauiControlsHandlers();
					DependencyService.SetToInitialized();

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST

					handlers.TryAddCompatibilityRenderer(typeof(BoxView), typeof(BoxRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Entry), typeof(EntryRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Editor), typeof(EditorRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Label), typeof(LabelRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Image), typeof(ImageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Button), typeof(ButtonRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ImageButton), typeof(ImageButtonRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(TableView), typeof(TableViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ListView), typeof(ListViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CollectionView), typeof(CollectionViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CarouselView), typeof(CarouselViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(IndicatorView), typeof(IndicatorViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Path), typeof(PathRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Ellipse), typeof(EllipseRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Line), typeof(LineRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Polyline), typeof(PolylineRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Polygon), typeof(PolygonRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Shapes.Rectangle), typeof(RectangleRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(RadioButton), typeof(RadioButtonRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Slider), typeof(SliderRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(WebView), typeof(WebViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(SearchBar), typeof(SearchBarRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Switch), typeof(SwitchRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(SwipeView), typeof(SwipeViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(DatePicker), typeof(DatePickerRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(TimePicker), typeof(TimePickerRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Picker), typeof(PickerRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Stepper), typeof(StepperRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ProgressBar), typeof(ProgressBarRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ScrollView), typeof(ScrollViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ActivityIndicator), typeof(ActivityIndicatorRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Frame), typeof(FrameRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CheckBox), typeof(CheckBoxRenderer));
#if !WINDOWS
					handlers.TryAddCompatibilityRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Shell), typeof(ShellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(OpenGLView), typeof(OpenGLViewRenderer));
#else
					handlers.TryAddCompatibilityRenderer(typeof(Layout), typeof(LayoutRenderer));
#endif
					handlers.TryAddCompatibilityRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Page), typeof(PageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(FlyoutPage), typeof(FlyoutPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(RefreshView), typeof(RefreshViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Cell), typeof(CellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ImageCell), typeof(ImageCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(EntryCell), typeof(EntryCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(TextCell), typeof(TextCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ViewCell), typeof(ViewCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(SwitchCell), typeof(SwitchCellRenderer));

					// This is for Layouts that currently don't work when assigned to LayoutHandler
					handlers.TryAddCompatibilityRenderer(typeof(ContentView), typeof(DefaultRenderer));
#if __IOS__
					handlers.TryAddCompatibilityRenderer(typeof(AbsoluteLayout), typeof(DefaultRenderer));
#endif


					DependencyService.Register<Xaml.ResourcesLoader>();
					DependencyService.Register<NativeBindingService>();
					DependencyService.Register<NativeValueConverterService>();
					DependencyService.Register<Deserializer>();
					DependencyService.Register<ResourcesProvider>();
					DependencyService.Register<Xaml.ValueConverterProvider>();

					// Shimmed renderers go directly to the registrar to load Image Handlers
					Internals.Registrar.Registered.Register(typeof(FileImageSource), typeof(FileImageSourceHandler));
					Internals.Registrar.Registered.Register(typeof(StreamImageSource), typeof(StreamImagesourceHandler));
					Internals.Registrar.Registered.Register(typeof(UriImageSource), typeof(ImageLoaderSourceHandler));
					Internals.Registrar.Registered.Register(typeof(FontImageSource), typeof(FontImageSourceHandler));


					Internals.Registrar.Registered.Register(typeof(Microsoft.Maui.EmbeddedFont), typeof(Microsoft.Maui.EmbeddedFontLoader));

#endif

#if __IOS__ || MACCATALYST
					Internals.Registrar.RegisterEffect("Xamarin", "ShadowEffect", typeof(ShadowEffect));
#endif
				})
				.ConfigureServices<MauiCompatBuilder>();

			return builder;
		}

		class MauiCompatBuilder : IMauiServiceBuilder
		{
			public void Configure(HostBuilderContext context, IServiceProvider services)
			{
#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
				CompatServiceProvider.SetServiceProvider(services);
#endif
			}

			public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
			{
#if WINDOWS
				if (!UI.Xaml.Application.Current.Resources.ContainsKey("MauiControlsPageControlStyle"))
				{
					var myResourceDictionary = new Microsoft.UI.Xaml.ResourceDictionary();
					myResourceDictionary.Source = new Uri("ms-appx:///Microsoft.Maui.Controls/Platform/Windows/Styles/Resources.xbf");
					Microsoft.UI.Xaml.Application.Current.Resources.MergedDictionaries.Add(myResourceDictionary);
				}
#endif
			}
		}
	}
}
