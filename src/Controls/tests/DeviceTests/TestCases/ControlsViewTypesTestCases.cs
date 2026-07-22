using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
#pragma warning disable CS0618 // Type or member is obsolete
            new object[] { typeof(Frame) },
#pragma warning restore CS0618 // Type or member is obsolete
            new object[] { typeof(GraphicsView) },
			new object[] { typeof(Image) },
			new object[] { typeof(ImageButton) },
			new object[] { typeof(IndicatorView) },
			new object[] { typeof(Label) },
			new object[] { typeof(Line) },
#pragma warning disable CS0618 // Type or member is obsolete
			new object[] { typeof(ListView) },
#pragma warning restore CS0618 // Type or member is obsolete
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
#pragma warning disable CS0618 // Type or member is obsolete
			new object[] { typeof(TableView) },
#pragma warning restore CS0618 // Type or member is obsolete
			new object[] { typeof(TimePicker) },
			new object[] { typeof(WebView) },
		};

		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ActivityIndicator))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Border))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(BoxView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Button))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(CarouselView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(CheckBox))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(CollectionView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ContentView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(DatePicker))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Editor))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Ellipse))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Entry))]
#pragma warning disable CS0618 // Type or member is obsolete
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Frame))]
#pragma warning restore CS0618 // Type or member is obsolete
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(GraphicsView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Image))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ImageButton))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(IndicatorView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Label))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Line))]
#pragma warning disable CS0618 // Type or member is obsolete
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ListView))]
#pragma warning restore CS0618 // Type or member is obsolete
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Picker))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Polygon))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Polyline))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ProgressBar))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RadioButton))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Rectangle))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RefreshView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(RoundRectangle))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(ScrollView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(SearchBar))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Slider))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Stepper))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(SwipeView))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(Switch))]
#pragma warning disable CS0618 // Type or member is obsolete
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(TableView))]
#pragma warning restore CS0618 // Type or member is obsolete
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(TimePicker))]
		[DynamicDependency(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor, typeof(WebView))]
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
#pragma warning disable CS0618 // Type or member is obsolete
				handlers.AddHandler<Frame, FrameRenderer>();
#pragma warning restore CS0618 // Type or member is obsolete
				handlers.AddHandler<GraphicsView, GraphicsViewHandler>();
				handlers.AddHandler<Label, LabelHandler>();
#pragma warning disable CS0618 // Type or member is obsolete
				handlers.AddHandler<ListView, ListViewRenderer>();
#pragma warning restore CS0618 // Type or member is obsolete
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
#pragma warning disable CS0618 // Type or member is obsolete
				handlers.AddHandler<TableView, TableViewRenderer>();
#pragma warning restore CS0618 // Type or member is obsolete
				handlers.AddHandler<TimePicker, TimePickerHandler>();
				handlers.AddHandler<WebView, WebViewHandler>();
			});
		}
	}
}