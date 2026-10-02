using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Diagnostics;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;

#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#elif WINDOWS
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Handlers.Items2;
#elif IOS || MACCATALYST
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items2;
#elif TIZEN
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
#endif

namespace Microsoft.Maui.Controls.Hosting;

public static partial class AppHostBuilderExtensions
{
	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	internal static MauiAppBuilder UseMauiPrimaryApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
		where TApp : class, IApplication
	{
#pragma warning disable RS0030 // Do not used banned APIs - don't want to use a factory method here
		builder.Services.TryAddSingleton<IApplication, TApp>();
#pragma warning restore RS0030
		builder.SetupDefaults();
		return builder;
	}

	/// <summary>
	/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
	/// </summary>
	/// <typeparam name="TApp">The type to use as the application.</typeparam>
	/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
	/// <param name="implementationFactory">A factory to create the specified <typeparamref name="TApp"/> using the services provided in a <see cref="IServiceProvider"/>.</param>
	/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
	internal static MauiAppBuilder UseMauiPrimaryApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
		where TApp : class, IApplication
	{
		builder.Services.TryAddSingleton<IApplication>(implementationFactory);
		builder.SetupDefaults();
		return builder;
	}

	internal static IMauiHandlersCollection AddControlsHandlers(this IMauiHandlersCollection handlersCollection)
	{
#if IOS || MACCATALYST
		handlersCollection.AddHandler<CollectionView>(static _ => new CollectionViewHandler2());
		handlersCollection.AddHandler<CarouselView>(static _ => new CarouselViewHandler2());
#elif WINDOWS
		if (RuntimeFeature.IsWindowsCollectionView2HandlerEnabled)
		{
			handlersCollection.AddHandler<CollectionView>(static _ => new CollectionViewHandler2());
		}
		else
		{
			handlersCollection.AddHandler<CollectionView>(static _ => new CollectionViewHandler());
		}
		handlersCollection.AddHandler<CarouselView>(static _ => new CarouselViewHandler());
#else
		handlersCollection.AddHandler<CollectionView>(static _ => new CollectionViewHandler());
		handlersCollection.AddHandler<CarouselView>(static _ => new CarouselViewHandler());
#endif
#if ANDROID
		if (RuntimeFeature.IsMaterial3Enabled)
		{
			handlersCollection.AddHandler<Label>(static _ => new LabelHandler2());
			handlersCollection.AddHandler<Editor>(static _ => new EditorHandler2());
			handlersCollection.AddHandler<Picker>(static _ => new PickerHandler2());
			handlersCollection.AddHandler<RadioButton>(static _ => new RadioButtonHandler2());
			handlersCollection.AddHandler<TimePicker>(static _ => new TimePickerHandler2());
			handlersCollection.AddHandler<Switch>(static _ => new SwitchHandler2());
			handlersCollection.AddHandler<ProgressBar>(static _ => new ProgressBarHandler2());
			handlersCollection.AddHandler<ActivityIndicator>(static _ => new ActivityIndicatorHandler2());
			handlersCollection.AddHandler<Image>(static _ => new ImageHandler2());
			handlersCollection.AddHandler<SearchBar>(static _ => new SearchBarHandler2());
			handlersCollection.AddHandler<Slider>(static _ => new SliderHandler2());
			handlersCollection.AddHandler<DatePicker>(static _ => new DatePickerHandler2());
			handlersCollection.AddHandler<Entry>(static _ => new EntryHandler2());
		}
		else
		{
			handlersCollection.AddHandler<Label>(static _ => new LabelHandler());
			handlersCollection.AddHandler<Editor>(static _ => new EditorHandler());
			handlersCollection.AddHandler<Picker>(static _ => new PickerHandler());
			handlersCollection.AddHandler<RadioButton>(static _ => new RadioButtonHandler());
			handlersCollection.AddHandler<TimePicker>(static _ => new TimePickerHandler());
			handlersCollection.AddHandler<Switch>(static _ => new SwitchHandler());
			handlersCollection.AddHandler<ProgressBar>(static _ => new ProgressBarHandler());
			handlersCollection.AddHandler<ActivityIndicator>(static _ => new ActivityIndicatorHandler());
			handlersCollection.AddHandler<Image>(static _ => new ImageHandler());
			handlersCollection.AddHandler<SearchBar>(static _ => new SearchBarHandler());
			handlersCollection.AddHandler<Slider>(static _ => new SliderHandler());
			handlersCollection.AddHandler<DatePicker>(static _ => new DatePickerHandler());
			handlersCollection.AddHandler<Entry>(static _ => new EntryHandler());
		}
#else
		handlersCollection.AddHandler<Label>(static _ => new LabelHandler());
		handlersCollection.AddHandler<Editor>(static _ => new EditorHandler());
		handlersCollection.AddHandler<Picker>(static _ => new PickerHandler());
		handlersCollection.AddHandler<RadioButton>(static _ => new RadioButtonHandler());
		handlersCollection.AddHandler<TimePicker>(static _ => new TimePickerHandler());
		handlersCollection.AddHandler<Switch>(static _ => new SwitchHandler());
		handlersCollection.AddHandler<ProgressBar>(static _ => new ProgressBarHandler());
		handlersCollection.AddHandler<ActivityIndicator>(static _ => new ActivityIndicatorHandler());
		handlersCollection.AddHandler<Image>(static _ => new ImageHandler());
		handlersCollection.AddHandler<SearchBar>(static _ => new SearchBarHandler());
		handlersCollection.AddHandler<Slider>(static _ => new SliderHandler());
		handlersCollection.AddHandler<DatePicker>(static _ => new DatePickerHandler());
		handlersCollection.AddHandler<Entry>(static _ => new EntryHandler());
#endif
		handlersCollection.AddHandler<Application>(static _ => new ApplicationHandler());
		handlersCollection.AddHandler<BoxView>(static _ => new BoxViewHandler());
		handlersCollection.AddHandler<Button>(static _ => new ButtonHandler());
		handlersCollection.AddHandler<CheckBox>(static _ => new CheckBoxHandler());
		handlersCollection.AddHandler<GraphicsView>(static _ => new GraphicsViewHandler());
		handlersCollection.AddHandler<Layout>(static _ => new LayoutHandler());
		handlersCollection.AddHandler<ScrollView>(static _ => new ScrollViewHandler());
		handlersCollection.AddHandler<Stepper>(static _ => new StepperHandler());
		handlersCollection.AddHandler<Page>(static _ => new PageHandler());
		handlersCollection.AddHandler<WebView>(static _ => new WebViewHandler());
		handlersCollection.AddHandler<HybridWebView>(static _ => new HybridWebViewHandler());

		handlersCollection.AddHandler<Border>(static _ => new BorderHandler());
		handlersCollection.AddHandler<IContentView>(static _ => new ContentViewHandler());
		handlersCollection.AddHandler<ContentView>(static _ => new ContentViewHandler());
		handlersCollection.AddHandler<Shapes.Ellipse>(static _ => new ShapeViewHandler());
		handlersCollection.AddHandler<Shapes.Line>(static _ => new LineHandler());
		handlersCollection.AddHandler<Shapes.Path>(static _ => new PathHandler());
		handlersCollection.AddHandler<Shapes.Polygon>(static _ => new PolygonHandler());
		handlersCollection.AddHandler<Shapes.Polyline>(static _ => new PolylineHandler());
		handlersCollection.AddHandler<Shapes.Rectangle>(static _ => new RectangleHandler());
		handlersCollection.AddHandler<Shapes.RoundRectangle>(static _ => new RoundRectangleHandler());
		handlersCollection.AddHandler<Window>(static _ => new WindowHandler());
		handlersCollection.AddHandler<ImageButton>(static _ => new ImageButtonHandler());
		handlersCollection.AddHandler<IndicatorView>(static _ => new IndicatorViewHandler());
		handlersCollection.AddHandler<RefreshView>(static _ => new RefreshViewHandler());
		handlersCollection.AddHandler<SwipeItem>(static _ => new SwipeItemMenuItemHandler());
		handlersCollection.AddHandler<SwipeView>(static _ => new SwipeViewHandler());

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
		handlersCollection.AddHandler<MenuBar>(static _ => new MenuBarHandler());
		handlersCollection.AddHandler<MenuFlyoutSubItem>(static _ => new MenuFlyoutSubItemHandler());
		handlersCollection.AddHandler<MenuFlyoutSeparator>(static _ => new MenuFlyoutSeparatorHandler());
		handlersCollection.AddHandler<MenuFlyoutItem>(static _ => new MenuFlyoutItemHandler());
		handlersCollection.AddHandler<MenuBarItem>(static _ => new MenuBarItemHandler());
#pragma warning restore CA1416

#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
#pragma warning disable CS0618 // Type or member is obsolete
#if ANDROID
		handlersCollection.AddHandler<ListView>(static services => new Handlers.Compatibility.ListViewRenderer(
			services.GetRequiredService<IMauiContext>().Context
				?? throw new InvalidOperationException("The Android context is required to create a ListViewRenderer.")));
#else
		handlersCollection.AddHandler<ListView>(static _ => new Handlers.Compatibility.ListViewRenderer());
#endif
#pragma warning restore CS0618 // Type or member is obsolete
#if !TIZEN
#pragma warning disable CS0618 // Type or member is obsolete
#if WINDOWS
		handlersCollection.AddHandler(typeof(Cell), typeof(Handlers.Compatibility.CellRenderer));
#else
		handlersCollection.AddHandler<Cell>(static _ => new Handlers.Compatibility.CellRenderer());
#endif
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		handlersCollection.AddHandler<ImageCell>(static _ => new Handlers.Compatibility.ImageCellRenderer());
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		handlersCollection.AddHandler<EntryCell>(static _ => new Handlers.Compatibility.EntryCellRenderer());
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		handlersCollection.AddHandler<TextCell>(static _ => new Handlers.Compatibility.TextCellRenderer());
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		handlersCollection.AddHandler<ViewCell>(static _ => new Handlers.Compatibility.ViewCellRenderer());
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		handlersCollection.AddHandler<SwitchCell>(static _ => new Handlers.Compatibility.SwitchCellRenderer());
#pragma warning restore CS0618 // Type or member is obsolete
#endif
#pragma warning disable CS0618 // Type or member is obsolete
#if ANDROID
		handlersCollection.AddHandler<TableView>(static services => new Handlers.Compatibility.TableViewRenderer(
			services.GetRequiredService<IMauiContext>().Context
				?? throw new InvalidOperationException("The Android context is required to create a TableViewRenderer.")));
#else
		handlersCollection.AddHandler<TableView>(static _ => new Handlers.Compatibility.TableViewRenderer());
#endif
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#if ANDROID
		handlersCollection.AddHandler<Frame>(static services => new Handlers.Compatibility.FrameRenderer(
			services.GetRequiredService<IMauiContext>().Context
				?? throw new InvalidOperationException("The Android context is required to create a FrameRenderer.")));
#else
		handlersCollection.AddHandler<Frame>(static _ => new Handlers.Compatibility.FrameRenderer());
#endif
#pragma warning restore CS0618 // Type or member is obsolete
#endif

#if WINDOWS || MACCATALYST
		handlersCollection.AddHandler<MenuFlyout>(static _ => new MenuFlyoutHandler());
#endif

#if IOS || MACCATALYST
		handlersCollection.AddHandler<NavigationPage>(static _ => new NavigationViewHandler());
		handlersCollection.AddHandler<TabbedPage>(static _ => new TabbedViewHandler());
		handlersCollection.AddHandler<FlyoutPage>(static _ => new FlyoutViewHandler());
#endif

#if ANDROID || IOS || MACCATALYST || TIZEN
		handlersCollection.AddHandler<SwipeItemView>(static _ => new SwipeItemViewHandler());
#endif

#if IOS || MACCATALYST
		if (RuntimeFeature.IsiOSShellHandlerEnabled)
		{
			handlersCollection.AddHandler<Shell>(static _ => new ShellHandler());
			handlersCollection.AddHandler<ShellItem>(static _ => new ShellItemHandler());
			handlersCollection.AddHandler<ShellSection>(static _ => new ShellSectionHandler());
			handlersCollection.AddHandler<ShellContent>(static _ => new ShellContentHandler());
		}
		else
		{
			handlersCollection.AddHandler<Shell>(static _ => new ShellRenderer());
		}
#elif WINDOWS
		handlersCollection.AddHandler<Shell>(static _ => new ShellHandler());
		handlersCollection.AddHandler<ShellItem>(static _ => new ShellItemHandler());
		handlersCollection.AddHandler<ShellSection>(static _ => new ShellSectionHandler());
		handlersCollection.AddHandler<ShellContent>(static _ => new ShellContentHandler());
#elif ANDROID || TIZEN
		handlersCollection.AddHandler<Shell>(static _ => new ShellHandler());
		handlersCollection.AddHandler<ShellItem>(static _ => new ShellItemHandler());
		handlersCollection.AddHandler<ShellSection>(static _ => new ShellSectionHandler());
#endif

#if WINDOWS || ANDROID || TIZEN
		handlersCollection.AddHandler<NavigationPage>(static _ => new NavigationViewHandler());
		handlersCollection.AddHandler<Toolbar>(static _ => new ToolbarHandler());
		handlersCollection.AddHandler<FlyoutPage>(static _ => new FlyoutViewHandler());
		handlersCollection.AddHandler<TabbedPage>(static _ => new TabbedViewHandler());
#endif

		return handlersCollection;
	}

	static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
	{
#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
		// initialize compatibility DependencyService
		DependencyService.SetToInitialized();

#pragma warning disable CS0612, CA1416 // Type or member is obsolete, 'ResourcesProvider' is unsupported on: 'iOS' 14.0 and later
		DependencyService.Register<ResourcesProvider>();
		DependencyService.Register<FontNamedSizeService>();
#pragma warning restore CS0612, CA1416 // Type or member is obsolete
#endif
		builder.Services.AddScoped(_ => new HideSoftInputOnTappedChangedManager());

		builder.ConfigureImageSourceHandlers();

		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddControlsHandlers();
		});

		builder.Services.AddScoped<IHybridWebViewTaskManager>(_ => new HybridWebViewTaskManager());

		builder.ConfigureMauiControlsDiagnostics();

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
			var dispatcher = services.GetRequiredApplicationDispatcher();

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

	static MauiAppBuilder ConfigureImageSourceHandlers(this MauiAppBuilder builder)
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
		Element.RemapIfNeeded();
		Application.RemapForControls();
		VisualElement.RemapIfNeeded();
		Button.RemapForControls();
		DatePicker.RemapForControls();
		RadioButton.RemapForControls();
		FlyoutPage.RemapForControls();
		Toolbar.RemapForControls();
		Window.RemapForControls();
		Editor.RemapForControls();
		Entry.RemapForControls();
		SwipeView.RemapForControls();
		Picker.RemapForControls();
		SearchBar.RemapForControls();
		Stepper.RemapForControls();
		TabbedPage.RemapForControls();
		TimePicker.RemapForControls();
		Layout.RemapForControls();
		ScrollView.RemapForControls();
		RefreshView.RemapForControls();
		Shape.RemapForControls();
		WebView.RemapForControls();
		ContentPage.RemapForControls();
		ImageButton.RemapForControls();

		Slider.RemapForControls();

#if IOS || MACCATALYST
		NavigationPage.RemapForControls();
#endif

		return builder;
	}
}
