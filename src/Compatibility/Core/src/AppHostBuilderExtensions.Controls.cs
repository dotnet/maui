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
#elif __IOS__
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using WebViewRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.WkWebViewRenderer;
using NavigationPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.NavigationRenderer;
using TabbedPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.TabbedRenderer;
using FlyoutPageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.PhoneFlyoutPageRenderer;
using RadioButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
#endif

using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Shapes;

namespace Microsoft.Maui.Controls
{
	public static class AppHostBuilderExtensions
	{
		public static IAppHostBuilder UseMauiControlsApp<TApp>(this IAppHostBuilder builder)
			where TApp : class, IApplication
		{
			builder.ConfigureServices((context, collection) =>
			{
				collection.AddSingleton<IApplication, TApp>();
			});

			builder.SetupDefaults();
			return builder;
		}

		public static IAppHostBuilder UseMauiControlsApp<TApp>(this IAppHostBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
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
					handlers.TryAddCompatibilityRenderer(typeof(OpenGLView), typeof(OpenGLViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CheckBox), typeof(CheckBoxRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(TabbedPage), typeof(TabbedPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(NavigationPage), typeof(NavigationPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Page), typeof(PageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(FlyoutPage), typeof(FlyoutPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(RefreshView), typeof(RefreshViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Shell), typeof(ShellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Cell), typeof(CellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ImageCell), typeof(ImageCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(EntryCell), typeof(EntryCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(TextCell), typeof(TextCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ViewCell), typeof(ViewCellRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(SwitchCell), typeof(SwitchCellRenderer));

	#if __IOS__ || MACCATALYST
					Internals.Registrar.RegisterEffect("Xamarin", "ShadowEffect", typeof(ShadowEffect));
#endif

#endif

#if __ANDROID__ || __IOS__ || WINDOWS || MACCATALYST
					DependencyService.Register<Xaml.ResourcesLoader>();
					DependencyService.Register<NativeBindingService>();
					DependencyService.Register<NativeValueConverterService>();
					DependencyService.Register<Deserializer>();
					DependencyService.Register<ResourcesProvider>();
					DependencyService.Register<Xaml.ValueConverterProvider>();
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
