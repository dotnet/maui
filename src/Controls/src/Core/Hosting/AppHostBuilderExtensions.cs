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

#if IOS || MACCATALYST
using Microsoft.Maui.Controls.Handlers.Items2;
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
		handlersCollection.AddHandler<CollectionView, CollectionViewHandler2>();
		handlersCollection.AddHandler<CarouselView, CarouselViewHandler2>();
#else
		handlersCollection.AddHandler<CollectionView, CollectionViewHandler>();
		handlersCollection.AddHandler<CarouselView, CarouselViewHandler>();
#endif
		handlersCollection.AddHandler<Application, ApplicationHandler>();
		handlersCollection.AddHandler<ActivityIndicator, ActivityIndicatorHandler>();
		handlersCollection.AddHandler<BoxView, BoxViewHandler>();
		handlersCollection.AddHandler<Button, ButtonHandler>();
		handlersCollection.AddHandler<DatePicker, DatePickerHandler>();
		handlersCollection.AddHandler<Editor, EditorHandler>();
		handlersCollection.AddHandler<Entry, EntryHandler>();
		handlersCollection.AddHandler<GraphicsView, GraphicsViewHandler>();
		handlersCollection.AddHandler<Image, ImageHandler>();
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
		if (RuntimeFeature.IsHybridWebViewSupported)
		{
			// NOTE: not registered under NativeAOT or TrimMode=Full scenarios
			handlersCollection.AddHandler<HybridWebView, HybridWebViewHandler>();
		}
		handlersCollection.AddHandler<Border, BorderHandler>();
		handlersCollection.AddHandler<IContentView, ContentViewHandler>();
		handlersCollection.AddHandler<ContentView, ContentViewHandler>();
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

#pragma warning disable CA1416 //  'MenuBarHandler', MenuFlyoutSubItemHandler, MenuFlyoutSubItemHandler, MenuBarItemHandler is only supported on: 'ios' 13.0 and later
		handlersCollection.AddHandler<MenuBar, MenuBarHandler>();
		handlersCollection.AddHandler<MenuFlyoutSubItem, MenuFlyoutSubItemHandler>();
		handlersCollection.AddHandler<MenuFlyoutSeparator, MenuFlyoutSeparatorHandler>();
		handlersCollection.AddHandler<MenuFlyoutItem, MenuFlyoutItemHandler>();
		handlersCollection.AddHandler<MenuBarItem, MenuBarItemHandler>();
#pragma warning restore CA1416

#if WINDOWS || MACCATALYST
		handlersCollection.AddHandler(typeof(MenuFlyout), typeof(MenuFlyoutHandler));
#endif

#if ANDROID || IOS || MACCATALYST || TIZEN
		handlersCollection.AddHandler<SwipeItemView, SwipeItemViewHandler>();
#endif

#if ANDROID || IOS || MACCATALYST
		handlersCollection.AddHandler<Shell, ShellRenderer>();
#elif WINDOWS
		handlersCollection.AddHandler<Shell, ShellHandler>();
		handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
		handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
		handlersCollection.AddHandler<ShellContent, ShellContentHandler>();
#elif TIZEN
		handlersCollection.AddHandler<Shell, ShellHandler>();
		handlersCollection.AddHandler<ShellItem, ShellItemHandler>();
		handlersCollection.AddHandler<ShellSection, ShellSectionHandler>();
#endif

#if WINDOWS || ANDROID || TIZEN || IOS || MACCATALYST
		handlersCollection.AddHandler<NavigationPage, NavigationViewHandler>();
		handlersCollection.AddHandler<Toolbar, ToolbarHandler>();
		handlersCollection.AddHandler<FlyoutPage, FlyoutViewHandler>();
		handlersCollection.AddHandler<TabbedPage, TabbedViewHandler>();
#endif

		return handlersCollection;
	}

	static MauiAppBuilder SetupDefaults(this MauiAppBuilder builder)
	{
		builder.Services.AddScoped(_ => new HideSoftInputOnTappedChangedManager());

		builder.ConfigureImageSourceHandlers();

		builder.ConfigureMauiHandlers(handlers =>
		{
			handlers.AddControlsHandlers();
		});

		// NOTE: not registered under NativeAOT or TrimMode=Full scenarios
		if (RuntimeFeature.IsHybridWebViewSupported)
		{
			builder.Services.AddScoped<IHybridWebViewTaskManager>(_ => new HybridWebViewTaskManager());
		}

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

		return builder;
	}
}
