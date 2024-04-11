using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory;

[Category(TestCategory.Memory)]
public class MemoryTests : ControlsHandlerTestBase
{
	void SetupBuilder()
	{
		EnsureHandlerCreated(builder =>
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<ActivityIndicator, ActivityIndicatorHandler>();
				handlers.AddHandler<Border, BorderHandler>();
				handlers.AddHandler<BoxView, BoxViewHandler>();
				handlers.AddHandler<CarouselView, CarouselViewHandler>();
				handlers.AddHandler<CollectionView, CollectionViewHandler>();
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
				handlers.AddHandler<IContentView, ContentViewHandler>();
				handlers.AddHandler<Image, ImageHandler>();
				handlers.AddHandler<ImageButton, ImageButtonHandler>();
				handlers.AddHandler<IndicatorView, IndicatorViewHandler>();
				handlers.AddHandler<RefreshView, RefreshViewHandler>();
				handlers.AddHandler<IScrollView, ScrollViewHandler>();
				handlers.AddHandler<SearchBar, SearchBarHandler>();
				handlers.AddHandler<Slider, SliderHandler>();
				handlers.AddHandler<Stepper, StepperHandler>();
				handlers.AddHandler<SwipeView, SwipeViewHandler>();
				handlers.AddHandler<Switch, SwitchHandler>();
				handlers.AddHandler<TableView, TableViewRenderer>();
				handlers.AddHandler<TimePicker, TimePickerHandler>();
				handlers.AddHandler<WebView, WebViewHandler>();
			});
		});
	}

	[Theory("Handler Does Not Leak")]
	[InlineData(typeof(ActivityIndicator))]
	[InlineData(typeof(Border))]
	[InlineData(typeof(BoxView))]
	[InlineData(typeof(CarouselView))]
	[InlineData(typeof(ContentView))]
	[InlineData(typeof(CheckBox))]
	[InlineData(typeof(DatePicker))]
	[InlineData(typeof(Entry))]
	[InlineData(typeof(Editor))]
	[InlineData(typeof(Frame))]
	[InlineData(typeof(GraphicsView))]
	[InlineData(typeof(Image))]
	[InlineData(typeof(ImageButton))]
	[InlineData(typeof(IndicatorView))]
	[InlineData(typeof(Label))]
	[InlineData(typeof(Picker))]
	[InlineData(typeof(Polygon))]
	[InlineData(typeof(Polyline))]
	[InlineData(typeof(RefreshView))]
	[InlineData(typeof(ScrollView))]
	[InlineData(typeof(SearchBar))]
	[InlineData(typeof(Slider))]
	[InlineData(typeof(Stepper))]
	[InlineData(typeof(SwipeView))]
	[InlineData(typeof(Switch))]
	[InlineData(typeof(TimePicker))]
	[InlineData(typeof(TableView))]
	[InlineData(typeof(WebView))]
	[InlineData(typeof(CollectionView))]
	public async Task HandlerDoesNotLeak(Type type)
	{
		SetupBuilder();

#if ANDROID
		// NOTE: skip certain controls on older Android devices
		if (type == typeof (DatePicker) && !OperatingSystem.IsAndroidVersionAtLeast(30))
				return;
#endif

#if IOS
		// NOTE: skip certain controls on older iOS devices
		if (type == typeof(WebView) && !OperatingSystem.IsIOSVersionAtLeast(16))
			return;
#endif

		WeakReference viewReference = null;
		WeakReference platformViewReference = null;
		WeakReference handlerReference = null;

		var observable = new ObservableCollection<int> { 1, 2, 3 };

		await InvokeOnMainThreadAsync(async () =>
		{
			var layout = new Grid();
			var view = (View)Activator.CreateInstance(type);
			layout.Add(view);
			if (view is ContentView content)
			{
				content.Content = new Label();
			}
			else if (view is ItemsView items)
			{
				items.ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					return label;
				});
				items.ItemsSource = observable;
			}
			else if (view is WebView webView)
			{
				webView.Source = new HtmlWebViewSource { Html = "<p>hi</p>" };
				await Task.Delay(1000);
			}
			var handler = CreateHandler<LayoutHandler>(layout);
			viewReference = new WeakReference(view);
			handlerReference = new WeakReference(view.Handler);
			platformViewReference = new WeakReference(view.Handler.PlatformView);
		});

		await AssertionExtensions.WaitForGC(viewReference, handlerReference, platformViewReference);
	}

#if IOS
	[Fact]
	public async Task ResignFirstResponderTouchGestureRecognizer()
	{
		WeakReference viewReference = null;
		WeakReference recognizerReference = null;

		await InvokeOnMainThreadAsync(() =>
		{
			var view = new UIKit.UIView();
			var recognizer = new Platform.ResignFirstResponderTouchGestureRecognizer(view);
			view.AddGestureRecognizer(recognizer);
			viewReference = new(view);
			recognizerReference = new(recognizer);
		});

		await AssertionExtensions.WaitForGC(viewReference, recognizerReference);
	}
#endif
}

