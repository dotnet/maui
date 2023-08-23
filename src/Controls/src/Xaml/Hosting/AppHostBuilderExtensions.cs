using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
#elif IOS || MACCATALYST
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Handlers.Compatibility;
#elif TIZEN
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
#endif

namespace Microsoft.Maui.Controls.Hosting
{
	public static partial class AppHostBuilderExtensions
	{
		/// <summary>
		/// Configures the <see cref="MauiAppBuilder"/> to use the specified <typeparamref name="TApp"/> as the main application type.
		/// </summary>
		/// <typeparam name="TApp">The type to use as the application.</typeparam>
		/// <param name="builder">The <see cref="MauiAppBuilder"/> to configure.</param>
		/// <returns>The configured <see cref="MauiAppBuilder"/>.</returns>
		public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder)
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
		public static MauiAppBuilder UseMauiApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApp>(this MauiAppBuilder builder, Func<IServiceProvider, TApp> implementationFactory)
			where TApp : class, IApplication
		{
			builder.Services.TryAddSingleton<IApplication>(implementationFactory);
			builder.SetupDefaults();
			return builder;
		}

		public static IMauiHandlersCollection AddMauiControlsHandlers(this IMauiHandlersCollection handlersCollection)
		{
			handlersCollection.AddHandler<CollectionView>(_ => new CollectionViewHandler());
			handlersCollection.AddHandler<CarouselView>(_ => new CarouselViewHandler());
			handlersCollection.AddHandler<Application>(_ => new ApplicationHandler());
			handlersCollection.AddHandler<ActivityIndicator>(_ => new ActivityIndicatorHandler());
			handlersCollection.AddHandler<BoxView>(_ => new BoxViewHandler());
			handlersCollection.AddHandler<Button>(_ => new ButtonHandler());
			handlersCollection.AddHandler<CheckBox>(_ => new CheckBoxHandler());
			handlersCollection.AddHandler<DatePicker>(_ => new DatePickerHandler());
			handlersCollection.AddHandler<Editor>(_ => new EditorHandler());
			handlersCollection.AddHandler<Entry>(_ => new EntryHandler());
			handlersCollection.AddHandler<GraphicsView>(_ => new GraphicsViewHandler());
			handlersCollection.AddHandler<Image>(_ => new ImageHandler());
			handlersCollection.AddHandler<Label>(_ => new LabelHandler());
			handlersCollection.AddHandler<Layout>(_ => new LayoutHandler());
			handlersCollection.AddHandler<Picker>(_ => new PickerHandler());
			handlersCollection.AddHandler<ProgressBar>(_ => new ProgressBarHandler());
			handlersCollection.AddHandler<ScrollView>(_ => new ScrollViewHandler());
			handlersCollection.AddHandler<SearchBar>(_ => new SearchBarHandler());
			handlersCollection.AddHandler<Slider>(_ => new SliderHandler());
			handlersCollection.AddHandler<Stepper>(_ => new StepperHandler());
			handlersCollection.AddHandler<Switch>(_ => new SwitchHandler());
			handlersCollection.AddHandler<TimePicker>(_ => new TimePickerHandler());
			handlersCollection.AddHandler<Page>(_ => new PageHandler());
			handlersCollection.AddHandler<WebView>(_ => new WebViewHandler());
			handlersCollection.AddHandler<Border>(_ => new BorderHandler());
			handlersCollection.AddHandler<IContentView>(_ => new ContentViewHandler());
			handlersCollection.AddHandler<Shapes.Ellipse>(_ => new ShapeViewHandler());
			handlersCollection.AddHandler<Shapes.Line>(_ => new LineHandler());
			handlersCollection.AddHandler<Shapes.Path>(_ => new PathHandler());
			handlersCollection.AddHandler<Shapes.Polygon>(_ => new PolygonHandler());
			handlersCollection.AddHandler<Shapes.Polyline>(_ => new PolylineHandler());
			handlersCollection.AddHandler<Shapes.Rectangle>(_ => new RectangleHandler());
			handlersCollection.AddHandler<Shapes.RoundRectangle>(_ => new RoundRectangleHandler());
			handlersCollection.AddHandler<Window>(_ => new WindowHandler());
			handlersCollection.AddHandler<ImageButton>(_ => new ImageButtonHandler());
			handlersCollection.AddHandler<IndicatorView>(_ => new IndicatorViewHandler());
			handlersCollection.AddHandler<RadioButton>(_ => new RadioButtonHandler());
			handlersCollection.AddHandler<RefreshView>(_ => new RefreshViewHandler());
			handlersCollection.AddHandler<SwipeItem>(_ => new SwipeItemMenuItemHandler());
			handlersCollection.AddHandler<SwipeView>(_ => new SwipeViewHandler());

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
			handlersCollection.AddHandler<MenuBar>(_ => new MenuBarHandler());
			handlersCollection.AddHandler<MenuFlyoutSubItem>(_ => new MenuFlyoutSubItemHandler());
			handlersCollection.AddHandler<MenuFlyoutSeparator>(_ => new MenuFlyoutSeparatorHandler());
			handlersCollection.AddHandler<MenuFlyoutItem>(_ => new MenuFlyoutItemHandler());
			handlersCollection.AddHandler<MenuBarItem>(_ => new MenuBarItemHandler());
#pragma warning restore CA1416

#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
			handlersCollection.AddHandler<ListView>(svc => new Handlers.Compatibility.ListViewRenderer(
	#if ANDROID
				svc.GetRequiredService<Android.Content.Context>()
	#endif
			));
#if !TIZEN
			handlersCollection.AddHandler<ImageCell>(_ => new Handlers.Compatibility.ImageCellRenderer());
			handlersCollection.AddHandler<EntryCell>(_ => new Handlers.Compatibility.EntryCellRenderer());
			handlersCollection.AddHandler<TextCell>(_ => new Handlers.Compatibility.TextCellRenderer());
			handlersCollection.AddHandler<ViewCell>(_ => new Handlers.Compatibility.ViewCellRenderer());
			handlersCollection.AddHandler<SwitchCell>(_ => new Handlers.Compatibility.SwitchCellRenderer());
#endif
			handlersCollection.AddHandler<TableView>(svc => new Handlers.Compatibility.TableViewRenderer(
	#if ANDROID
				svc.GetRequiredService<Android.Content.Context>()
	#endif
			));
			handlersCollection.AddHandler<Frame>(svc => new Handlers.Compatibility.FrameRenderer(
	#if ANDROID
				svc.GetRequiredService<Android.Content.Context>()
	#endif
			));
#endif

#if WINDOWS || MACCATALYST
			handlersCollection.AddHandler<MenuFlyout>(_ => new MenuFlyoutHandler());
#endif

#if IOS || MACCATALYST
			handlersCollection.AddHandler<NavigationPage>(_ => new Handlers.Compatibility.NavigationRenderer());
			handlersCollection.AddHandler<TabbedPage>(_ => new Handlers.Compatibility.TabbedRenderer());
			handlersCollection.AddHandler<FlyoutPage>(_ => new Handlers.Compatibility.PhoneFlyoutPageRenderer());
#endif

#if ANDROID || IOS || MACCATALYST || TIZEN
			handlersCollection.AddHandler<SwipeItemView>(_ => new SwipeItemViewHandler());
#if ANDROID || IOS || MACCATALYST
			handlersCollection.AddHandler<Shell>(_ => new ShellRenderer());
#else
			handlersCollection.AddHandler<Shell>(_ => new ShellHandler());
			handlersCollection.AddHandler<ShellItem>(_ => new ShellItemHandler());
			handlersCollection.AddHandler<ShellSection>(_ => new ShellSectionHandler());
#endif
#endif
#if WINDOWS || ANDROID || TIZEN
			handlersCollection.AddHandler<NavigationPage>(_ => new NavigationViewHandler());
			handlersCollection.AddHandler<Toolbar>(_ => new ToolbarHandler());
			handlersCollection.AddHandler<FlyoutPage>(_ => new FlyoutViewHandler());
			handlersCollection.AddHandler<TabbedPage>(_ => new TabbedViewHandler());
#endif

#if WINDOWS
			handlersCollection.AddHandler<ShellItem>(_ => new ShellItemHandler());
			handlersCollection.AddHandler<ShellSection>(_ => new ShellSectionHandler());
			handlersCollection.AddHandler<ShellContent>(_ => new ShellContentHandler());
			handlersCollection.AddHandler<Shell>(_ => new ShellHandler());
#endif
			return handlersCollection;
		}

		static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
		{
#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
			// initialize compatibility DependencyService
			DependencyService.SetToInitialized();
			DependencyService.Register<Xaml.ResourcesLoader>();
			DependencyService.Register<Xaml.ValueConverterProvider>();
			DependencyService.Register<PlatformSizeService>();

#pragma warning disable CS0612, CA1416 // Type or member is obsolete, 'ResourcesProvider' is unsupported on: 'iOS' 14.0 and later
			DependencyService.Register<ResourcesProvider>();
			DependencyService.Register<FontNamedSizeService>();
#pragma warning restore CS0612, CA1416 // Type or member is obsolete
#endif
			builder.Services.AddScoped<HideSoftInputOnTappedChangedManager>();
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
			Element.RemapForControls();
			Application.RemapForControls();
			VisualElement.RemapForControls();
			Label.RemapForControls();
			Button.RemapForControls();
			CheckBox.RemapForControls();
			DatePicker.RemapForControls();
			RadioButton.RemapForControls();
			FlyoutPage.RemapForControls();
			Toolbar.RemapForControls();
			Window.RemapForControls();
			Editor.RemapForControls();
			Entry.RemapForControls();
			Picker.RemapForControls();
			SearchBar.RemapForControls();
			TabbedPage.RemapForControls();
			TimePicker.RemapForControls();
			Layout.RemapForControls();
			ScrollView.RemapForControls();
			RefreshView.RemapForControls();
			Shape.RemapForControls();
			WebView.RemapForControls();
			ContentPage.RemapForControls();

			return builder;
		}
	}
}