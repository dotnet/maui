using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

#if IOS || MACCATALYST
using FlyoutViewHandler = Microsoft.Maui.Controls.Handlers.Compatibility.PhoneFlyoutPageRenderer;
#endif

namespace Microsoft.Maui.DeviceTests.TestCases
{
	public class ControlsViewTypesTestCases : IEnumerable<object[]>
	{
		private readonly List<object[]> _data = new()
		{
			new object[] { typeof(ActivityIndicator) },
			new object[] { typeof(Border) },
			new object[] { typeof(BoxView) },
			new object[] { typeof(Button) },
			new object[] { typeof(CarouselView) },
			new object[] { typeof(CheckBox) },
			new object[] { typeof(CollectionView) },
			new object[] { typeof(ContentView) },
			new object[] { typeof(DatePicker) },
			new object[] { typeof(Editor) },
			new object[] { typeof(Ellipse) },
			new object[] { typeof(Entry) },
			new object[] { typeof(Frame) },
			new object[] { typeof(GraphicsView) },
			new object[] { typeof(Image) },
			new object[] { typeof(ImageButton) },
			new object[] { typeof(IndicatorView) },
			new object[] { typeof(Label) },
			new object[] { typeof(Line) },
			new object[] { typeof(ListView) },
			new object[] { typeof(Picker) },
			new object[] { typeof(Polygon) },
			new object[] { typeof(Polyline) },
			new object[] { typeof(ProgressBar) },
			new object[] { typeof(RadioButton) },
			new object[] { typeof(Rectangle) },
			new object[] { typeof(RefreshView) },
			new object[] { typeof(RoundRectangle) },
			new object[] { typeof(ScrollView) },
			new object[] { typeof(SearchBar) },
			new object[] { typeof(Slider) },
			new object[] { typeof(Stepper) },
			new object[] { typeof(SwipeView) },
			new object[] { typeof(Switch) },
			new object[] { typeof(TableView) },
			new object[] { typeof(TimePicker) },
			new object[] { typeof(WebView) },
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public static void Setup(MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<ActivityIndicator, ActivityIndicatorHandler>();
				handlers.AddHandler<Border, BorderHandler>();
				handlers.AddHandler<BoxView, BoxViewHandler>();
				handlers.AddHandler<Button, ButtonHandler>();
				handlers.AddHandler<CarouselView, CarouselViewHandler>();
				handlers.AddHandler<CollectionView, CollectionViewHandler>();
				handlers.AddHandler<ContentView, ContentViewHandler>();
				handlers.AddHandler<CheckBox, CheckBoxHandler>();
				handlers.AddHandler<DatePicker, DatePickerHandler>();
				handlers.AddHandler<Entry, EntryHandler>();
				handlers.AddHandler<Editor, EditorHandler>();
				handlers.AddHandler<Frame, FrameRenderer>();
				handlers.AddHandler<GraphicsView, GraphicsViewHandler>();
				handlers.AddHandler<Label, LabelHandler>();
				handlers.AddHandler<ListView, ListViewRenderer>();
				handlers.AddHandler<Picker, PickerHandler>();
				handlers.AddHandler<Polygon, PolygonHandler>();
				handlers.AddHandler<Polyline, PolylineHandler>();
				handlers.AddHandler<Ellipse, ShapeViewHandler>();
				handlers.AddHandler<Line, LineHandler>();
				handlers.AddHandler<Path, PathHandler>();
				handlers.AddHandler<Polygon, PolygonHandler>();
				handlers.AddHandler<Polyline, PolylineHandler>();
				handlers.AddHandler<Rectangle, RectangleHandler>();
				handlers.AddHandler<RoundRectangle, RoundRectangleHandler>();
				handlers.AddHandler<ProgressBar, ProgressBarHandler>();
				handlers.AddHandler<SearchBar, SearchBarHandler>();
				handlers.AddHandler<IContentView, ContentViewHandler>();
				handlers.AddHandler<Image, ImageHandler>();
				handlers.AddHandler<ImageButton, ImageButtonHandler>();
				handlers.AddHandler<IndicatorView, IndicatorViewHandler>();
				handlers.AddHandler<RefreshView, RefreshViewHandler>();
				handlers.AddHandler<IScrollView, ScrollViewHandler>();
				handlers.AddHandler<Slider, SliderHandler>();
				handlers.AddHandler<Stepper, StepperHandler>();
				handlers.AddHandler<SwipeView, SwipeViewHandler>();
				handlers.AddHandler<Switch, SwitchHandler>();
				handlers.AddHandler<TableView, TableViewRenderer>();
				handlers.AddHandler<TimePicker, TimePickerHandler>();
				handlers.AddHandler<WebView, WebViewHandler>();
			});
		}
	}
}