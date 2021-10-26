using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		static readonly Dictionary<Type, Type> DefaultMauiControlHandlers = new Dictionary<Type, Type>
		{
#if __IOS__ || __ANDROID__
			{ typeof(CollectionView), typeof(CollectionViewHandler) },
#endif

#if WINDOWS
			{ typeof(CollectionView), typeof(CollectionViewHandler) },
#endif

#if WINDOWS || __ANDROID__
			{ typeof(Shell), typeof(ShellHandler) },
#endif
			{ typeof(Application), typeof(ApplicationHandler) },
			{ typeof(ActivityIndicator), typeof(ActivityIndicatorHandler) },
			{ typeof(BoxView), typeof(ShapeViewHandler) },
			{ typeof(Button), typeof(ButtonHandler) },
			{ typeof(CheckBox), typeof(CheckBoxHandler) },
			{ typeof(DatePicker), typeof(DatePickerHandler) },
			{ typeof(Editor), typeof(EditorHandler) },
			{ typeof(Entry), typeof(EntryHandler) },
			{ typeof(GraphicsView), typeof(GraphicsViewHandler) },
			{ typeof(Image), typeof(ImageHandler) },
			{ typeof(Label), typeof(LabelHandler) },
			{ typeof(Layout), typeof(LayoutHandler) },
			{ typeof(Picker), typeof(PickerHandler) },
			{ typeof(ProgressBar), typeof(ProgressBarHandler) },
			{ typeof(ScrollView), typeof(ScrollViewHandler) },
			{ typeof(SearchBar), typeof(SearchBarHandler) },
			{ typeof(Slider), typeof(SliderHandler) },
			{ typeof(Stepper), typeof(StepperHandler) },
			{ typeof(Switch), typeof(SwitchHandler) },
			{ typeof(TimePicker), typeof(TimePickerHandler) },
			{ typeof(Page), typeof(PageHandler) },
			{ typeof(WebView), typeof(WebViewHandler) },
			{ typeof(Border), typeof(BorderHandler) },
			{ typeof(IContentView), typeof(ContentViewHandler) },
			{ typeof(Shapes.Ellipse), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Line), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Path), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Polygon), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Polyline), typeof(ShapeViewHandler) },
			{ typeof(Shapes.Rectangle), typeof(ShapeViewHandler) },
			{ typeof(Shapes.RoundRectangle), typeof(ShapeViewHandler) },
			{ typeof(Window), typeof(WindowHandler) },
			{ typeof(ImageButton), typeof(ImageButtonHandler) },
			{ typeof(IndicatorView), typeof(IndicatorViewHandler) },
#if __ANDROID__ || __IOS__
			{ typeof(RefreshView), typeof(RefreshViewHandler) },
			
#endif
#if WINDOWS || ANDROID
			{ typeof(NavigationPage), typeof(NavigationViewHandler) },
			{ typeof(Toolbar), typeof(Controls.Handlers.ToolbarHandler) },
#endif
#if __ANDROID__
			{ typeof(TabbedPage), typeof(Controls.Handlers.TabbedPageHandler) },
#endif
		};

		public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
		{
			return handlersCollection.AddHandlers(DefaultMauiControlHandlers);
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

		internal static MauiAppBuilder ConfigureControlsLifecycleEvents(this MauiAppBuilder builder)
		{
			builder.ConfigureLifecycleEvents(lifeCycle =>
			{
				// We want the remap to happen as early as possible in the application lifecycle
				// If users start interacting with mappers before they have been remapped it can cause unexpected behavior.
				// We also want the Remap to happen before `InitializeComponent` runs because `InitializeComponent`
				// will cause the remapped static mappers to instantiate pre-maturally.
				// The remapping needs to happen in a specific order otherwise the remappings at the VisualElement and Element levels
				// won't get added into the chain on the concrete controls correctly
				// Application.RemapMappers() is called on the ctor of Application so these primarily serve as
				// just in case hooks if the user isn't using our Application class
#if ANDROID
				lifeCycle.AddAndroid(android => android.OnApplicationCreating((_) => { Application.RemapMappers(); }));
#elif WINDOWS
				lifeCycle.AddWindows(windows => windows.OnLaunching((_, __) => { Application.RemapMappers(); }));
#elif IOS
				lifeCycle.AddiOS(iOS => iOS.WillFinishLaunching((_, __) => { Application.RemapMappers(); return true; }));
#endif
			});

			return builder;
		}
	}
}
