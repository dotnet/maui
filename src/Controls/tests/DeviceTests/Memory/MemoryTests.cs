using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.Stubs;
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
				handlers.AddHandler<Shape, ShapeViewHandler>();
				handlers.AddHandler<Entry, EntryHandler>();
				handlers.AddHandler<EntryCell, EntryCellRenderer>();
				handlers.AddHandler<Editor, EditorHandler>();
#pragma warning disable CS0618 // Type or member is obsolete
				handlers.AddHandler<Frame, FrameRenderer>();
#pragma warning restore CS0618 // Type or member is obsolete
				handlers.AddHandler<GraphicsView, GraphicsViewHandler>();
				handlers.AddHandler<HybridWebView, HybridWebViewHandler>();
				handlers.AddHandler<Label, LabelHandler>();
				handlers.AddHandler<ListView, ListViewRenderer>();
				handlers.AddHandler<Layout, LayoutHandler>();
				handlers.AddHandler<Picker, PickerHandler>();
				handlers.AddHandler<Polygon, PolygonHandler>();
				handlers.AddHandler<Polyline, PolylineHandler>();
				handlers.AddHandler<IContentView, ContentViewHandler>();
				handlers.AddHandler<Image, ImageHandler>();
				handlers.AddHandler<ImageButton, ImageButtonHandler>();
				handlers.AddHandler<ImageCell, ImageCellRenderer>();
				handlers.AddHandler<IndicatorView, IndicatorViewHandler>();
				handlers.AddHandler<RadioButton, RadioButtonHandler>();
				handlers.AddHandler<RefreshView, RefreshViewHandler>();
				handlers.AddHandler<IScrollView, ScrollViewHandler>();
				handlers.AddHandler<SearchBar, SearchBarHandler>();
				handlers.AddHandler<Slider, SliderHandler>();
				handlers.AddHandler<Stepper, StepperHandler>();
				handlers.AddHandler<SwipeView, SwipeViewHandler>();
				handlers.AddHandler<Switch, SwitchHandler>();
				handlers.AddHandler<SwitchCell, SwitchCellRenderer>();
				handlers.AddHandler<TableView, TableViewRenderer>();
				handlers.AddHandler<TextCell, TextCellRenderer>();
				handlers.AddHandler<TimePicker, TimePickerHandler>();
				handlers.AddHandler<Toolbar, ToolbarHandler>();
				handlers.AddHandler<WebView, WebViewHandler>();
				handlers.AddHandler<ViewCell, ViewCellRenderer>();
#if IOS || MACCATALYST
				handlers.AddHandler<NavigationPage, NavigationRenderer>();
				handlers.AddHandler<TabbedPage, TabbedRenderer>();
#else
				handlers.AddHandler<NavigationPage, NavigationViewHandler>();
				handlers.AddHandler<TabbedPage, TabbedViewHandler>();
#endif
			});
		});
	}

	[Theory("Pages Do Not Leak")]
	[InlineData(typeof(ContentPage))]
	[InlineData(typeof(NavigationPage))]
	[InlineData(typeof(TabbedPage))]
	public async Task PagesDoNotLeak(Type type)
	{
		SetupBuilder();

		var references = new List<WeakReference>();
		var navPage = new NavigationPage(new ContentPage { Title = "Page 1" });

		await CreateHandlerAndAddToWindow(new Window(navPage), async () =>
		{
			var page = (Page)Activator.CreateInstance(type);
			var pageToWaitFor = page;
			if (page is ContentPage contentPage)
			{
				contentPage.Content = new Label();
			}
			else if (page is NavigationPage navigationPage)
			{
				pageToWaitFor = new ContentPage { Content = new Label() };
				await navigationPage.PushAsync(pageToWaitFor);
			}
			else if (page is TabbedPage tabbedPage)
			{
				pageToWaitFor = new ContentPage { Content = new Label() };
				tabbedPage.Children.Add(pageToWaitFor);
			}

			await navPage.Navigation.PushModalAsync(page);

			references.Add(new(page));
			references.Add(new(page.Handler));
			references.Add(new(page.Handler.PlatformView));

			await OnLoadedAsync(pageToWaitFor);
			if (pageToWaitFor != page)
			{
				references.Add(new(pageToWaitFor));
				references.Add(new(pageToWaitFor.Handler));
				references.Add(new(pageToWaitFor.Handler.PlatformView));
			}

			await navPage.Navigation.PopModalAsync();
		});

		await AssertionExtensions.WaitForGC(references.ToArray());
	}

	[Theory("Handler Does Not Leak")]
	[InlineData(typeof(ActivityIndicator))]
	[InlineData(typeof(Border))]
	[InlineData(typeof(BoxView))]
	[InlineData(typeof(CarouselView))]
	[InlineData(typeof(ContentView))]
	[InlineData(typeof(CheckBox))]
	// [InlineData(typeof(DatePicker))] - This test was moved to MemoryTests.cs inside Appium
	[InlineData(typeof(Ellipse))]
	[InlineData(typeof(Entry))]
	[InlineData(typeof(Editor))]
#pragma warning disable CS0618 // Type or member is obsolete
	[InlineData(typeof(Frame))]
#pragma warning restore CS0618 // Type or member is obsolete
	[InlineData(typeof(GraphicsView))]
	[InlineData(typeof(Grid))]
	[InlineData(typeof(HybridWebView))]
	[InlineData(typeof(Image))]
	[InlineData(typeof(ImageButton))]
	[InlineData(typeof(IndicatorView))]
	[InlineData(typeof(Line))]
	[InlineData(typeof(Label))]
	[InlineData(typeof(ListView))]
	[InlineData(typeof(Path))]
	[InlineData(typeof(Picker))]
	[InlineData(typeof(Polygon))]
	[InlineData(typeof(Polyline))]
	[InlineData(typeof(RadioButton))]
	[InlineData(typeof(Rectangle))]
	[InlineData(typeof(RefreshView))]
	[InlineData(typeof(RoundRectangle))]
	[InlineData(typeof(ScrollView))]
	[InlineData(typeof(SearchBar))]
	[InlineData(typeof(Slider))]
	[InlineData(typeof(Stepper))]
	[InlineData(typeof(SwipeView))]
	[InlineData(typeof(Switch))]
	[InlineData(typeof(TimePicker))]
	[InlineData(typeof(TableView))]
	//[InlineData(typeof(WebView))] - This test was moved to MemoryTests.cs inside Appium
	[InlineData(typeof(CollectionView))]
	public async Task HandlerDoesNotLeak(Type type)
	{
		SetupBuilder();

#if ANDROID
		// NOTE: skip certain controls on older Android devices
		if ((type == typeof(DatePicker) || type == typeof(ListView)) && !OperatingSystem.IsAndroidVersionAtLeast(30))
				return;

		if (type == typeof(HybridWebView) && !OperatingSystem.IsAndroidVersionAtLeast(24))
		{
			return;
		}
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
			if (view is Border border)
			{
				border.StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) };
				border.Content = new Label();
			}
			else if (view is ContentView content)
			{
				content.Content = new Label();
			}
			else if (view is ListView listView)
			{
				listView.ItemTemplate = new DataTemplate(() =>
				{
					var cell = new TextCell();
					cell.SetBinding(TextCell.TextProperty, ".");
					return cell;
				});
				listView.ItemsSource = observable;
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
			else if (view is HybridWebView hybridWebView)
			{
				hybridWebView.HybridRoot = "HybridTestRoot";
				await Task.Delay(1000);
			}
			else if (view is TemplatedView templated)
			{
				templated.ControlTemplate = new ControlTemplate(() =>
					new Border
					{
						StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) },
						Content = new Grid { Children = { new Ellipse(), new ContentPresenter() } }
					});
			}
			var handler = CreateHandler<LayoutHandler>(layout);
			viewReference = new WeakReference(view);
			handlerReference = new WeakReference(view.Handler);
			platformViewReference = new WeakReference(view.Handler.PlatformView);
		});

		await AssertionExtensions.WaitForGC(viewReference, handlerReference, platformViewReference);
	}

	[Theory("Gesture Does Not Leak")]
	[InlineData(typeof(DragGestureRecognizer))]
	[InlineData(typeof(DropGestureRecognizer))]
	[InlineData(typeof(PanGestureRecognizer))]
	[InlineData(typeof(PinchGestureRecognizer))]
	[InlineData(typeof(PointerGestureRecognizer))]
	[InlineData(typeof(SwipeGestureRecognizer))]
	[InlineData(typeof(TapGestureRecognizer))]
	public async Task GestureDoesNotLeak(Type type)
	{
		SetupBuilder();

		WeakReference viewReference = null;
		WeakReference handlerReference = null;

		var observable = new ObservableCollection<int> { 1 };
		var navPage = new NavigationPage(new ContentPage { Title = "Page 1" });

		await CreateHandlerAndAddToWindow(new Window(navPage), async () =>
		{
			await navPage.Navigation.PushAsync(new ContentPage
			{
				Content = new CollectionView
				{
					ItemTemplate = new DataTemplate(() =>
					{
						var view = new Label
						{
							GestureRecognizers =
							{
								(GestureRecognizer)Activator.CreateInstance(type)
							}
						};
						view.SetBinding(Label.TextProperty, ".");

						viewReference = new WeakReference(view);
						handlerReference = new WeakReference(view.Handler);

						return view;
					}),
					ItemsSource = observable
				}
			});

			await navPage.Navigation.PopAsync();
		});

		await AssertionExtensions.WaitForGC(viewReference, handlerReference);
	}

	[Theory("Cells Do Not Leak")]
	[InlineData(typeof(TextCell))]
	[InlineData(typeof(EntryCell))]
	[InlineData(typeof(ImageCell))]
	[InlineData(typeof(SwitchCell))]
	[InlineData(typeof(ViewCell))]
	public async Task CellsDoNotLeak(Type type)
	{
		SetupBuilder();

		var references = new List<WeakReference>();
		var observable = new ObservableCollection<int> { 1 };
		var navPage = new NavigationPage(new ContentPage { Title = "Page 1" });

		await CreateHandlerAndAddToWindow(new Window(navPage), async () =>
		{
			await navPage.Navigation.PushAsync(new ContentPage
			{
				Content = new ListView
				{
					ItemTemplate = new DataTemplate(() =>
					{
						var cell = (Cell)Activator.CreateInstance(type);
						if (cell is ViewCell viewCell)
						{
							viewCell.View = new Label();
						}
						references.Add(new(cell));
						return cell;
					}),
					ItemsSource = observable
				}
			});

			Assert.NotEmpty(references);
			foreach (var reference in references.ToArray())
			{
				if (reference.Target is Cell cell)
				{
					Assert.NotNull(cell.Handler);
					references.Add(new(cell.Handler));
					Assert.NotNull(cell.Handler.PlatformView);
					references.Add(new(cell.Handler.PlatformView));
				}
			}

			await navPage.Navigation.PopAsync();
		});

		await AssertionExtensions.WaitForGC(references.ToArray());
	}

	[Fact("BindableLayout Does Not Leak")]
	public async Task BindableLayoutDoesNotLeak()
	{
		SetupBuilder();

		var references = new List<WeakReference>();
		var observable = new ObservableCollection<object>
		{
			new { Name = "One" },
			new { Name = "Two" },
			new { Name = "Three" },
			new { Name = "Four" },
		};

		var layout = new VerticalStackLayout();

		{
			BindableLayout.SetItemsSource(layout, observable);
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() =>
			{
				var radio = new RadioButton
				{
					ControlTemplate = new ControlTemplate(() =>
					{
						var radio = new RadioButton
						{
							ControlTemplate = new ControlTemplate(() =>
							{
								var ellipse = new Ellipse();
								references.Add(new(ellipse));

								return new HorizontalStackLayout
								{
									Children =
									{
										ellipse,
										new ContentPresenter(),
									}
								};
							})
						};
						radio.SetBinding(RadioButton.ContentProperty, "Name");
						return radio;
					})
				};
				radio.SetBinding(RadioButton.ContentProperty, "Name");
				return radio;
			}));
			var page = new ContentPage { Content = layout };
			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(page), async _ =>
			{
				await OnLoadedAsync(page);
				BindableLayout.SetItemsSource(layout, new ObservableCollection<object>(observable.Take(2)));
				page.Content = null;
			});
		}

		// 4 Ellipses total: last 2 should not leak, first 2 should still be in the layout & alive
		Assert.Equal(4, references.Count);
		await AssertionExtensions.WaitForGC(references[2], references[3]);
	}

	[Fact("Window Does Not Leak")]
	public async Task WindowDoesNotLeak()
	{
		SetupBuilder();

		var references = new List<WeakReference>();

		{
			var page = new ContentPage();
			var window = new Window(page);
			await CreateHandlerAndAddToWindow(window, async () =>
			{
				await OnLoadedAsync(page);
				references.Add(new(window));
				references.Add(new(window.Handler));

				// NOTE: the PlatformView in this case remains alive in the test application:
				// Activity on Android, Microsoft.UI.Xaml.Window on Windows, etc.
				//references.Add(new(window.Handler.PlatformView));

				if (MauiContext.Services.GetService<IApplication>() is ApplicationStub app)
				{
					app.SetWindow(null);
				}
			});
		}

		await AssertionExtensions.WaitForGC(references.ToArray());
	}

	[Fact("VisualDiagnosticsOverlay Does Not Leak"
#if IOS || MACCATALYST
		, Skip = "Fails with 'MauiContext should have been set on parent.'"
#endif
	)]
	public async Task VisualDiagnosticsOverlayDoesNotLeak()
	{
		SetupBuilder();

		var window = new WindowStub();
		var overlay = new VisualDiagnosticsOverlay(window);
		var references = new List<WeakReference>();

		{
			await InvokeOnMainThreadAsync(async () =>
			{
				var page = new ContentPage();
				window.Content = page;
				await CreateHandlerAsync(page);
				overlay.Initialize();
				references.Add(new(page));
				references.Add(new(page.Handler));
				references.Add(new(page.Handler.PlatformView));

				window.Content = null;
			});
		}

		await AssertionExtensions.WaitForGC(references.ToArray());
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

