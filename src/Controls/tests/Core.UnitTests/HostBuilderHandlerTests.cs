using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class HostBuilderHandlerTests
	{
		[Fact]
		public void DefaultHandlersAreRegistered()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.NotNull(handlers);
			var handler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(handler);
			Assert.Equal(typeof(ButtonHandler), handler.GetType());
		}

		[Fact]
		public void CanSpecifyHandler()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button>(static _ => new ButtonHandlerStub()))
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.NotNull(handlers);

			var specificHandler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(specificHandler);
			Assert.Equal(typeof(ButtonHandlerStub), specificHandler.GetType());
		}

		[Theory]
		[InlineData(typeof(ContentButton))]
		[InlineData(typeof(IContentButton))]
		public void ContentButtonHandlerTypeMatchesActivation(Type viewType)
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.Equal(typeof(ContentButtonHandler), handlers.GetHandlerType(viewType));
			Assert.IsType<ContentButtonHandler>(handlers.GetHandler(viewType));
		}

		[Fact]
		public void TryAddHandlerDoesNotReplaceEnabledElementHandlerDefault()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.TryAddHandler<Button>(static _ => new ButtonHandlerStub()))
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandler>(handlers.GetHandler(typeof(Button)));
		}

		[Fact]
		public void TryAddHandlerRegistersDerivedTypeWithoutDirectElementHandlerDefault()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.ConfigureMauiHandlers(handlers => handlers.TryAddHandler<FancyButton>(static _ => new ButtonHandlerStub()))
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandlerStub>(handlers.GetHandler(typeof(FancyButton)));
		}

		[Fact]
		public void TryAddHandlerRegistersWhenElementHandlerDefaultsAreDisabled()
		{
			var mauiApp = MauiApp.CreateBuilder(useDefaults: false)
				.ConfigureMauiHandlers(handlers => handlers.TryAddHandler<Button>(static _ => new ButtonHandlerStub()))
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandlerStub>(handlers.GetHandler(typeof(Button)));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void AddMauiControlsHandlersPreservesDefaultsPolicy(bool useControlsDefaults)
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			if (useControlsDefaults)
				builder.UseMauiApp<ApplicationStub>();

			using var mauiApp = builder
				.ConfigureMauiHandlers(handlers =>
				{
					var registrationCount = handlers.Count;
					Assert.Same(handlers, handlers.AddMauiControlsHandlers());
					Assert.Equal(registrationCount, handlers.Count);

#pragma warning disable RS0030 // This test verifies compatibility with type-based handler registrations.
					handlers.TryAddHandler(typeof(Button), typeof(ButtonHandlerStub));
#pragma warning restore RS0030

					registrationCount = handlers.Count;
					Assert.Same(handlers, handlers.AddMauiControlsHandlers());
					Assert.Equal(registrationCount, handlers.Count);
				})
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			var expectedHandlerType = useControlsDefaults ? typeof(ButtonHandler) : typeof(ButtonHandlerStub);
			Assert.Equal(expectedHandlerType, handlers.GetHandlerType(typeof(Button)));
			Assert.Equal(expectedHandlerType, handlers.GetHandler(typeof(Button)).GetType());
		}

		[Theory]
		[MemberData(nameof(BuiltInHandlerTypes))]
		public void VariousControlsGetCorrectHandler(Type viewType, Type handlerType)
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var specificHandler = handlers.GetHandler(viewType);

			Assert.NotNull(specificHandler);
			Assert.Equal(handlerType, specificHandler.GetType());
		}

		public static TheoryData<Type, Type> BuiltInHandlerTypes => new()
		{
			{ typeof(ActivityIndicator), typeof(ActivityIndicatorHandler) },
			{ typeof(Border), typeof(BorderHandler) },
			{ typeof(BoxView), typeof(BoxViewHandler) },
			{ typeof(Button), typeof(ButtonHandler) },
			{ typeof(ContentButton), typeof(ContentButtonHandler) },
			{ typeof(IContentButton), typeof(ContentButtonHandler) },
			{ typeof(CheckBox), typeof(CheckBoxHandler) },
			{ typeof(ContentPage), typeof(PageHandler) },
			{ typeof(ContentView), typeof(ContentViewHandler) },
			{ typeof(DatePicker), typeof(DatePickerHandler) },
			{ typeof(Editor), typeof(EditorHandler) },
			{ typeof(Entry), typeof(EntryHandler) },
			{ typeof(GraphicsView), typeof(GraphicsViewHandler) },
			{ typeof(Image), typeof(ImageHandler) },
			{ typeof(ImageButton), typeof(ImageButtonHandler) },
			{ typeof(IndicatorView), typeof(IndicatorViewHandler) },
			{ typeof(Label), typeof(LabelHandler) },
			{ typeof(Layout), typeof(LayoutHandler) },
			{ typeof(Page), typeof(PageHandler) },
			{ typeof(Picker), typeof(PickerHandler) },
			{ typeof(ProgressBar), typeof(ProgressBarHandler) },
			{ typeof(RadioButton), typeof(RadioButtonHandler) },
			{ typeof(RefreshView), typeof(RefreshViewHandler) },
			{ typeof(ScrollView), typeof(ScrollViewHandler) },
			{ typeof(SearchBar), typeof(SearchBarHandler) },
			{ typeof(Slider), typeof(SliderHandler) },
			{ typeof(Stepper), typeof(StepperHandler) },
			{ typeof(SwipeItem), typeof(SwipeItemMenuItemHandler) },
#if ANDROID || IOS || MACCATALYST || TIZEN
			{ typeof(SwipeItemView), typeof(SwipeItemViewHandler) },
#else
			{ typeof(SwipeItemView), typeof(ContentViewHandler) },
#endif
			{ typeof(SwipeView), typeof(SwipeViewHandler) },
			{ typeof(Switch), typeof(SwitchHandler) },
			{ typeof(TemplatedView), typeof(ContentViewHandler) },
			{ typeof(TimePicker), typeof(TimePickerHandler) },
			{ typeof(WebView), typeof(WebViewHandler) },
			{ typeof(MyTestCustomTemplatedView), typeof(ContentViewHandler) },
			{ typeof(Application), typeof(ApplicationHandler) },
			{ typeof(Window), typeof(WindowHandler) },
			{ typeof(Path), typeof(PathHandler) },
			{ typeof(Polygon), typeof(PolygonHandler) },
			{ typeof(Polyline), typeof(PolylineHandler) },
			{ typeof(RoundRectangle), typeof(RoundRectangleHandler) },
			{ typeof(Rectangle), typeof(RectangleHandler) },
			{ typeof(Line), typeof(LineHandler) },
			{ typeof(Ellipse), typeof(ShapeViewHandler) },
#if IOS || MACCATALYST
			{ typeof(CarouselView), typeof(Handlers.Items2.CarouselViewHandler2) },
			{ typeof(CollectionView), typeof(Handlers.Items2.CollectionViewHandler2) },
#else
			{ typeof(CarouselView), typeof(Handlers.Items.CarouselViewHandler) },
			{ typeof(CollectionView), typeof(Handlers.Items.CollectionViewHandler) },
#endif
#if WINDOWS || ANDROID || IOS || MACCATALYST || TIZEN
			{ typeof(FlyoutPage), typeof(FlyoutViewHandler) },
			{ typeof(NavigationPage), typeof(NavigationViewHandler) },
			{ typeof(TabbedPage), typeof(TabbedViewHandler) },
#endif
#if WINDOWS || ANDROID || TIZEN
			{ typeof(Toolbar), typeof(ToolbarHandler) },
#endif
#if IOS || MACCATALYST
			{ typeof(Shell), typeof(Handlers.Compatibility.ShellRenderer) },
#elif ANDROID || WINDOWS || TIZEN
			{ typeof(Shell), typeof(Handlers.ShellHandler) },
#endif
#if WINDOWS
			{ typeof(ShellContent), typeof(Handlers.ShellContentHandler) },
#endif
#if ANDROID || WINDOWS || TIZEN
			{ typeof(ShellItem), typeof(Handlers.ShellItemHandler) },
			{ typeof(ShellSection), typeof(Handlers.ShellSectionHandler) },
#endif
#if WINDOWS || MACCATALYST
			{ typeof(MenuBar), typeof(MenuBarHandler) },
			{ typeof(MenuBarItem), typeof(MenuBarItemHandler) },
			{ typeof(MenuFlyout), typeof(MenuFlyoutHandler) },
			{ typeof(MenuFlyoutItem), typeof(MenuFlyoutItemHandler) },
			{ typeof(MenuFlyoutSeparator), typeof(MenuFlyoutSeparatorHandler) },
			{ typeof(MenuFlyoutSubItem), typeof(MenuFlyoutSubItemHandler) },
#endif
#if WINDOWS || ANDROID || IOS || MACCATALYST
			{ typeof(EntryCell), typeof(Handlers.Compatibility.EntryCellRenderer) },
			{ typeof(ImageCell), typeof(Handlers.Compatibility.ImageCellRenderer) },
			{ typeof(SwitchCell), typeof(Handlers.Compatibility.SwitchCellRenderer) },
			{ typeof(TextCell), typeof(Handlers.Compatibility.TextCellRenderer) },
			{ typeof(ViewCell), typeof(Handlers.Compatibility.ViewCellRenderer) },
#endif
#if ANDROID || IOS || MACCATALYST
			{ typeof(ListView), typeof(Handlers.Compatibility.ListViewRenderer) },
			{ typeof(TableView), typeof(Handlers.Compatibility.TableViewRenderer) },
#endif
		};

		[Fact]
		public void RegisteredBaseControlHandlerOverridesInheritedElementHandlerAttribute()
		{
			var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers =>
				{
#pragma warning disable RS0030 // This test verifies compatibility with type-based handler registrations.
					handlers.AddHandler(typeof(Button), typeof(ButtonHandlerStub));
#pragma warning restore RS0030
				})
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandlerStub>(handlers.GetHandler(typeof(FancyButton)));
			Assert.Same(typeof(ButtonHandlerStub), handlers.GetHandlerType(typeof(FancyButton)));
		}

		[Fact]
		public void RegistrationForDerivedClassOverridesInheritedElementHandlerAttribute()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers =>
				{
#pragma warning disable RS0030 // GetHandlerType can only report type-based registrations.
					handlers.AddHandler(typeof(FancyButton), typeof(ButtonHandlerStub));
#pragma warning restore RS0030
				})
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandlerStub>(handlers.GetHandler(typeof(FancierButton)));
			Assert.Same(typeof(ButtonHandlerStub), handlers.GetHandlerType(typeof(FancierButton)));
		}

		[Fact]
		public void InterfaceRegistrationDoesNotOverrideInheritedElementHandlerAttribute()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IButton>(static _ => new ButtonHandlerStub()))
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			// The built-in Button default was a concrete registration, which outranked interface registrations.
			Assert.IsType<ButtonHandler>(handlers.GetHandler(typeof(Button)));
			Assert.IsType<ButtonHandler>(handlers.GetHandler(typeof(FancyButton)));
			Assert.Same(typeof(ButtonHandler), handlers.GetHandlerType(typeof(FancyButton)));
		}

		[Fact]
		public void BroaderClassRegistrationDoesNotOverrideNearerInheritedElementHandlerAttribute()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<View>(static _ => new ButtonHandlerStub()))
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandler>(handlers.GetHandler(typeof(FancyButton)));
			Assert.Same(typeof(ButtonHandler), handlers.GetHandlerType(typeof(FancyButton)));
		}

		[Fact]
		public void ContentViewDefaultOutranksMoreGeneralInterfaceRegistration()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<IView>(static _ => new ButtonHandlerStub()))
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ContentViewHandler>(handlers.GetHandler(typeof(MyTestCustomTemplatedView)));
			Assert.Same(typeof(ContentViewHandler), handlers.GetHandlerType(typeof(MyTestCustomTemplatedView)));
		}

		[Fact]
		public void ContentButtonDefaultOutranksBroaderInterfaceRegistrations()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IButton>(static _ => new ButtonHandlerStub());
					handlers.AddHandler<IContentView>(static _ => new ButtonHandlerStub());
				})
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ContentButtonHandler>(handlers.GetHandler(typeof(IContentButton)));
			Assert.Same(typeof(ContentButtonHandler), handlers.GetHandlerType(typeof(IContentButton)));
		}

		[Fact]
		public void ContentViewRegistrationOverridesContentViewDefault()
		{
			using var mauiApp = MauiApp.CreateBuilder()
				.UseMauiApp<ApplicationStub>()
				.ConfigureMauiHandlers(handlers =>
				{
#pragma warning disable RS0030 // GetHandlerType can only report type-based registrations.
					handlers.AddHandler(typeof(IContentView), typeof(ButtonHandlerStub));
#pragma warning restore RS0030
				})
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandlerStub>(handlers.GetHandler(typeof(MyTestCustomTemplatedView)));
			Assert.Same(typeof(ButtonHandlerStub), handlers.GetHandlerType(typeof(MyTestCustomTemplatedView)));
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void TryAddHandlerRespectsContentInterfaceDefaults(bool useControlsDefaults)
		{
			var builder = MauiApp.CreateBuilder(useDefaults: false);
			if (useControlsDefaults)
				builder.UseMauiApp<ApplicationStub>();

			using var mauiApp = builder
				.ConfigureMauiHandlers(handlers =>
				{
#pragma warning disable RS0030 // GetHandlerType can only report type-based registrations.
					handlers.TryAddHandler(typeof(IContentView), typeof(ButtonHandlerStub));
#pragma warning restore RS0030
					handlers.TryAddHandler<IContentButton>(static _ => new ButtonHandlerStub());
				})
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			var expectedContentViewHandler = useControlsDefaults ? typeof(ContentViewHandler) : typeof(ButtonHandlerStub);
			Assert.IsType(expectedContentViewHandler, handlers.GetHandler(typeof(MyTestCustomTemplatedView)));
			Assert.Same(expectedContentViewHandler, handlers.GetHandlerType(typeof(MyTestCustomTemplatedView)));

			var expectedContentButtonHandler = useControlsDefaults ? typeof(ContentButtonHandler) : typeof(ButtonHandlerStub);
			Assert.IsType(expectedContentButtonHandler, handlers.GetHandler(typeof(IContentButton)));
		}

		class MyTestCustomTemplatedView : TemplatedView
		{
		}

		class FancyButton : Button
		{
		}

		class FancierButton : FancyButton
		{
		}
	}
}
