#nullable enable
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls.Compatibility;

#if __ANDROID__
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using FrameRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.FrameRenderer;
using LabelRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.LabelRenderer;
using ImageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ImageRenderer;
using ButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ButtonRenderer;
#elif WINDOWS
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using BoxRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.BoxViewBorderRenderer;
using CellRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.TextCellRenderer;
using Deserializer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsSerializer;
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
#elif __IOS__
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using WebViewRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.WkWebViewRenderer;
using NavigationPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.NavigationRenderer;
using TabbedPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.TabbedRenderer;
using FlyoutPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.PhoneFlyoutPageRenderer;
using RadioButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
#endif

using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.LifecycleEvents;

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
						   .OnCreate((a, b) =>
						   {
							   // This just gets Forms Compat bits setup with what it needs
							   // to initialize the first view. MauiContext hasn't been initialized at this point
							   // so we setup one that will look exactly the same just
							   // to make legacy Forms bits happy
							   var services = MauiApplication.Current.Services;
							   MauiContext mauiContext = new MauiContext(services, a);
							   ActivationState state = new ActivationState(mauiContext, b);
							   Forms.Init(new ActivationState(mauiContext, b), new InitializationOptions() { Flags = InitializationFlags.SkipRenderers });
						   })
						   .OnPostCreate((_,b) =>
						   {
							   // This calls Init again so that the MauiContext that's part of
							   // Forms.Init matches the rest of the maui application
							   var mauiApp = MauiApplication.Current.Application;
							   if (mauiApp.Windows.Count > 0)
							   {
								   var window = mauiApp.Windows[0];
								   var mauiContext = window.Handler?.MauiContext ?? window.View.Handler?.MauiContext;

								   if (mauiContext != null)
								   {
									   Forms.Init(new ActivationState(mauiContext, b));
								   }
							   }
						   }));
#elif __IOS__
				events.AddiOS(iOS =>
				{
					iOS.WillFinishLaunching((x, y) =>
					{
						MauiContext mauiContext = new MauiContext(MauiUIApplicationDelegate.Current.Services);
						Forms.Init(new ActivationState(mauiContext), new InitializationOptions() { Flags = InitializationFlags.SkipRenderers });
						return true;
					});

					iOS.FinishedLaunching((x,y) =>
					{
						// This calls Init again so that the MauiContext that's part of
						// Forms.Init matches the rest of the maui application
						var mauiApp = MauiUIApplicationDelegate.Current.Application;
						if (mauiApp.Windows.Count > 0)
						{
							var window = mauiApp.Windows[0];
							var mauiContext = window.Handler?.MauiContext ?? window.View.Handler?.MauiContext;

							if (mauiContext != null)
							{
								Forms.Init(new ActivationState(mauiContext));
							}
						}
						return true;
					});
				});
#elif WINDOWS
				events.AddWindows(windows => windows
							.OnLaunching((_, args) =>
							{								
								// We need to call Forms.Init so the Window and Root Page can new up successfully
								// The dispatcher that's inside of Forms.Init needs to be setup before the initial 
								// window and root page start creating
								// Inside OnLaunched we grab the MauiContext that's on the window so we can have the correct
								// MauiContext inside Forms
								MauiContext mauiContext = new MauiContext(MauiWinUIApplication.Current.Services);
								ActivationState state = new ActivationState(mauiContext, args);
								Forms.Init(state, new InitializationOptions() { Flags = InitializationFlags.SkipRenderers });
							})
						   .OnLaunched((_, args) =>
						   {
							   // This calls Init again so that the MauiContext that's part of
							   // Forms.Init matches the rest of the maui application
							   var mauiApp = MauiWinUIApplication.Current.Application;
							   if (mauiApp.Windows.Count > 0)
							   {
								   var window = mauiApp.Windows[0];
								   var mauiContext = window.Handler?.MauiContext ?? window.View.Handler?.MauiContext;

								   if (mauiContext != null)
								   {
									   Forms.Init(new ActivationState(mauiContext, args));
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

					Forms.RenderersRegistered();
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
					handlers.TryAddCompatibilityRenderer(typeof(Rectangle), typeof(RectangleRenderer));
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
					DependencyService.Register<Xaml.ResourcesLoader>();
					DependencyService.Register<NativeBindingService>();
					DependencyService.Register<NativeValueConverterService>();
					DependencyService.Register<Deserializer>();
					DependencyService.Register<ResourcesProvider>();
					DependencyService.Register<Xaml.ValueConverterProvider>();
#endif

#if __IOS__ || MACCATALYST
					Internals.Registrar.RegisterEffect("Xamarin", "ShadowEffect", typeof(ShadowEffect));
#endif
				})
				.ConfigureServices(ConfigureNativeServices);


			return builder;
		}

		static void ConfigureNativeServices(HostBuilderContext arg1, IServiceCollection arg2)
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
