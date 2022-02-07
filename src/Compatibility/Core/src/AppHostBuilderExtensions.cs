#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using FrameRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.FrameRenderer;
using LabelRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.LabelRenderer;
using ImageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ImageRenderer;
using ButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ButtonRenderer;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.DefaultRenderer;
#elif WINDOWS
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using BoxRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.BoxViewBorderRenderer;
using CellRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.TextCellRenderer;
using Deserializer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsSerializer;
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using StreamImagesourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.UWP.StreamImageSourceHandler;
using ImageLoaderSourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.UWP.UriImageSourceHandler;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.DefaultRenderer;
#elif __IOS__
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using WebViewRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.WkWebViewRenderer;
using RadioButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
	public static class MauiAppBuilderExtensions
	{
		public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
			where TApp : class, IApplication
		{
			builder.Services.TryAddSingleton<IApplication, TApp>();
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

		static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
		{
#if ANDROID || IOS || WINDOWS
			// initialize compatibility DependencyService
			DependencyService.SetToInitialized();
			DependencyService.Register<Xaml.ResourcesLoader>();
			DependencyService.Register<NativeBindingService>();
			DependencyService.Register<NativeValueConverterService>();
			DependencyService.Register<Deserializer>();
			DependencyService.Register<ResourcesProvider>();
			DependencyService.Register<Xaml.ValueConverterProvider>();
#endif

			builder.ConfigureCompatibilityLifecycleEvents();
			builder.ConfigureImageSourceHandlers();
			builder
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddMauiControlsHandlers();

#if ANDROID || IOS || WINDOWS
					handlers.AddHandler(typeof(ListView), typeof(Handlers.Compatibility.ListViewRenderer));
					handlers.AddHandler(typeof(Cell), typeof(Handlers.Compatibility.CellRenderer));
					handlers.AddHandler(typeof(ImageCell), typeof(Handlers.Compatibility.ImageCellRenderer));
					handlers.AddHandler(typeof(EntryCell), typeof(Handlers.Compatibility.EntryCellRenderer));
					handlers.AddHandler(typeof(TextCell), typeof(Handlers.Compatibility.TextCellRenderer));
					handlers.AddHandler(typeof(ViewCell), typeof(Handlers.Compatibility.ViewCellRenderer));
					handlers.AddHandler(typeof(SwitchCell), typeof(Handlers.Compatibility.SwitchCellRenderer));
					handlers.AddHandler(typeof(TableView), typeof(Handlers.Compatibility.TableViewRenderer));
					handlers.AddHandler(typeof(Frame), typeof(Handlers.Compatibility.FrameRenderer));

					handlers.TryAddCompatibilityRenderer(typeof(BoxView), typeof(BoxRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Entry), typeof(EntryRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Editor), typeof(EditorRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Label), typeof(LabelRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Image), typeof(ImageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Button), typeof(ButtonRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(ImageButton), typeof(ImageButtonRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CollectionView), typeof(CollectionViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(CarouselView), typeof(CarouselViewRenderer));
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
					handlers.TryAddCompatibilityRenderer(typeof(CheckBox), typeof(CheckBoxRenderer));
#if !WINDOWS
					handlers.TryAddCompatibilityRenderer(typeof(Shell), typeof(ShellRenderer));
#if !(MACCATALYST || MACOS)
					handlers.TryAddCompatibilityRenderer(typeof(OpenGLView), typeof(OpenGLViewRenderer));
#endif
#else
					handlers.TryAddCompatibilityRenderer(typeof(Layout), typeof(LayoutRenderer));
#endif
					handlers.TryAddCompatibilityRenderer(typeof(CarouselPage), typeof(CarouselPageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Page), typeof(PageRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(RefreshView), typeof(RefreshViewRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer));

					handlers.TryAddCompatibilityRenderer(typeof(Microsoft.Maui.Controls.Compatibility.RelativeLayout), typeof(DefaultRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Microsoft.Maui.Controls.Compatibility.AbsoluteLayout), typeof(DefaultRenderer));

					// Shimmed renderers go directly to the registrar to load Image Handlers
					Internals.Registrar.Registered.Register(typeof(FileImageSource), typeof(FileImageSourceHandler));
					Internals.Registrar.Registered.Register(typeof(StreamImageSource), typeof(StreamImagesourceHandler));
					Internals.Registrar.Registered.Register(typeof(UriImageSource), typeof(ImageLoaderSourceHandler));
					Internals.Registrar.Registered.Register(typeof(FontImageSource), typeof(FontImageSourceHandler));
					Internals.Registrar.Registered.Register(typeof(Microsoft.Maui.EmbeddedFont), typeof(Microsoft.Maui.EmbeddedFontLoader));
#endif

#if IOS || MACCATALYST
					Internals.Registrar.RegisterEffect("Xamarin", "ShadowEffect", typeof(ShadowEffect));
					handlers.AddHandler(typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer));
					handlers.AddHandler(typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer));
					handlers.AddHandler(typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer));
#endif
				});

			builder.AddMauiCompat();
			builder.RemapForControls();

			return builder;
		}

		private static MauiAppBuilder AddMauiCompat(this MauiAppBuilder builder)
		{
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiCompatInitializer>());
			return builder;
		}

		class MauiCompatInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
#if WINDOWS
				var dictionaries = UI.Xaml.Application.Current?.Resources?.MergedDictionaries;
				if (dictionaries != null)
				{
					// WinUI
					AddLibraryResources<UI.Xaml.Controls.XamlControlsResources>();

					// Microsoft.Maui
					AddLibraryResources("MicrosoftMauiCoreIncluded", "ms-appx:///Microsoft.Maui/Platform/Windows/Styles/Resources.xbf");

					// Microsoft.Maui.Controls
					AddLibraryResources("MicrosoftMauiControlsIncluded", "ms-appx:///Microsoft.Maui.Controls/Platform/Windows/Styles/Resources.xbf");

					// Microsoft.Maui.Controls.Compatibility
					AddLibraryResources("MicrosoftMauiControlsCompatibilityIncluded", "ms-appx:///Microsoft.Maui.Controls.Compatibility/Windows/Resources.xbf");
				}
#endif
			}

#if WINDOWS
			static void AddLibraryResources(string key, string uri)
			{
				var resources = UI.Xaml.Application.Current?.Resources;
				if (resources == null)
					return;

				var dictionaries = resources.MergedDictionaries;
				if (dictionaries == null)
					return;

				if (!resources.ContainsKey(key))
				{
					dictionaries.Add(new UI.Xaml.ResourceDictionary
					{
						Source = new Uri(uri)
					});
				}
			}

			static void AddLibraryResources<T>()
				where T : UI.Xaml.ResourceDictionary, new()
			{
				var dictionaries = UI.Xaml.Application.Current?.Resources?.MergedDictionaries;
				if (dictionaries == null)
					return;

				var found = false;
				foreach (var dic in dictionaries)
				{
					if (dic is T)
					{
						found = true;
						break;
					}
				}

				if (!found)
				{
					var dic = new T();
					dictionaries.Add(dic);
				}
			}
#endif
		}
	}
}
