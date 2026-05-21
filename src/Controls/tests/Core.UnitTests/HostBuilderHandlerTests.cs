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
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, ButtonHandlerStub>())
				.Build();

			Assert.NotNull(mauiApp.Services);
			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();
			Assert.NotNull(handlers);

			var specificHandler = handlers.GetHandler(typeof(Button));

			Assert.NotNull(specificHandler);
			Assert.Equal(typeof(ButtonHandlerStub), specificHandler.GetType());
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
			{ typeof(FlyoutPage), typeof(Handlers.Compatibility.PhoneFlyoutPageRenderer) },
			{ typeof(NavigationPage), typeof(Handlers.Compatibility.NavigationRenderer) },
			{ typeof(TabbedPage), typeof(Handlers.Compatibility.TabbedRenderer) },
#else
			{ typeof(CarouselView), typeof(Handlers.Items.CarouselViewHandler) },
			{ typeof(CollectionView), typeof(Handlers.Items.CollectionViewHandler) },
#endif
#if WINDOWS || ANDROID || TIZEN
			{ typeof(FlyoutPage), typeof(FlyoutViewHandler) },
			{ typeof(NavigationPage), typeof(NavigationViewHandler) },
			{ typeof(TabbedPage), typeof(TabbedViewHandler) },
			{ typeof(Toolbar), typeof(ToolbarHandler) },
#endif
#if ANDROID || IOS || MACCATALYST
			{ typeof(Shell), typeof(Handlers.Compatibility.ShellRenderer) },
#elif WINDOWS || TIZEN
			{ typeof(Shell), typeof(Handlers.ShellHandler) },
#endif
#if WINDOWS
			{ typeof(ShellContent), typeof(Handlers.ShellContentHandler) },
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
				.ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, ButtonHandlerStub>())
				.Build();

			var handlers = mauiApp.Services.GetRequiredService<IMauiHandlersFactory>();

			Assert.IsType<ButtonHandlerStub>(handlers.GetHandler(typeof(FancyButton)));
			Assert.Same(typeof(ButtonHandlerStub), handlers.GetHandlerType(typeof(FancyButton)));
		}

		class MyTestCustomTemplatedView : TemplatedView
		{
		}

		class FancyButton : Button
		{
		}
	}
}
