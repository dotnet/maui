#if ANDROID
using Microsoft.Maui.Handlers;
using AView = Android.Views.View;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7814, "Vertical scrolling not working for CarouselView and CustomLayouts", PlatformAffected.Android)]
public class Issue7814 : TestContentPage
{
	const string VerticalOffsetPrefix = "VerticalScrollY";
	const string HorizontalOffsetPrefix = "HorizontalScrollX";
	const string TouchParentPositionPrefix = "TouchParentPosition";
	const string TouchStatusPrefix = "TouchStatus";

	Label _verticalOffsetLabel = null!;
	Label _horizontalOffsetLabel = null!;
	Label _touchParentPositionLabel = null!;
	Label _touchStatusLabel = null!;

	protected override void Init()
	{
		_verticalOffsetLabel = CreateOffsetLabel("Issue7814VerticalScrollYLabel", VerticalOffsetPrefix);
		_horizontalOffsetLabel = CreateOffsetLabel("Issue7814HorizontalScrollXLabel", HorizontalOffsetPrefix);
		_touchParentPositionLabel = CreateOffsetLabel("Issue7814TouchParentPositionLabel", TouchParentPositionPrefix);
		_touchStatusLabel = CreateOffsetLabel("Issue7814TouchStatusLabel", TouchStatusPrefix);

		Grid.SetColumn(_horizontalOffsetLabel, 1);

		var outerScrollView = new ScrollView
		{
			AutomationId = "Issue7814OuterScrollView",
			Orientation = ScrollOrientation.Vertical,
			Content = CreateScrollableContent()
		};
		outerScrollView.Scrolled += (_, e) => UpdateOffset(_verticalOffsetLabel, VerticalOffsetPrefix, e.ScrollY);
		Grid.SetRow(outerScrollView, 1);

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				new Grid
				{
					Padding = new Thickness(12, 8),
					RowDefinitions =
					{
						new RowDefinition(GridLength.Auto),
						new RowDefinition(GridLength.Auto)
					},
					ColumnDefinitions =
					{
						new ColumnDefinition(GridLength.Star),
						new ColumnDefinition(GridLength.Star)
					},
					Children =
					{
						_verticalOffsetLabel,
						Column(_horizontalOffsetLabel, 1),
						Row(_touchParentPositionLabel, 1),
						Column(Row(_touchStatusLabel, 1), 1)
					}
				},
				outerScrollView
			}
		};
	}

	View CreateScrollableContent()
	{
		var carouselView = new CarouselView
		{
			AutomationId = "Issue7814CarouselView",
			HeightRequest = 420,
			Loop = false,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemsSource = Enumerable.Range(1, 5).Select(index => $"Carousel item {index}").ToList(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 24,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Border
				{
					Margin = new Thickness(12),
					BackgroundColor = Colors.YellowGreen,
					Stroke = Colors.Green,
					StrokeThickness = 2,
					Content = label
				};
			})
		};

		var horizontalItems = new HorizontalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(12, 0)
		};

		foreach (var index in Enumerable.Range(1, 8))
		{
			horizontalItems.Children.Add(new Border
			{
				WidthRequest = 330,
				HeightRequest = 300,
				Stroke = Colors.Green,
				StrokeThickness = 2,
				Content = new Label
				{
					Text = $"Horizontal item {index}",
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				}
			});
		}

		var horizontalScrollView = new ScrollView
		{
			AutomationId = "Issue7814HorizontalScrollView",
			Orientation = ScrollOrientation.Horizontal,
			HeightRequest = 220,
			Content = horizontalItems
		};
		horizontalScrollView.Scrolled += (_, e) => UpdateOffset(_horizontalOffsetLabel, HorizontalOffsetPrefix, e.ScrollX);

		var touchClaimRegressionParent = CreateTouchClaimRegressionParent();

		return new VerticalStackLayout
		{
			Spacing = 16,
			Padding = new Thickness(0, 12),
			Children =
			{
				new Label
				{
					Text = "Touch and scroll vertically from the carousel, then horizontally from the list below.",
					Margin = new Thickness(12, 0)
				},
				carouselView,
				new Label
				{
					Text = "Horizontal ScrollView",
					FontSize = 20,
					Margin = new Thickness(12, 0)
				},
				horizontalScrollView,
				new Label
				{
					Text = "Touch-claiming row in a vertical CollectionView inside a horizontal parent",
					FontSize = 20,
					Margin = new Thickness(12, 0)
				},
				touchClaimRegressionParent,
				new BoxView
				{
					HeightRequest = 900,
					Color = Colors.Transparent
				}
			}
		};
	}

	View CreateTouchClaimRegressionParent()
	{
		var carouselView = new CarouselView
		{
			AutomationId = "Issue7814TouchClaimHorizontalParent",
			HeightRequest = 380,
			Loop = false,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
			ItemsSource = Enumerable.Range(1, 3).ToList(),
			ItemTemplate = new DataTemplate(CreateTouchClaimRegressionCarouselItem)
		};
		carouselView.PositionChanged += (_, e) => UpdateOffset(_touchParentPositionLabel, TouchParentPositionPrefix, e.CurrentPosition);

		return carouselView;
	}

	View CreateTouchClaimRegressionCarouselItem()
	{
		var collectionView = new CollectionView
		{
			AutomationId = "Issue7814TouchClaimCollectionView",
			WidthRequest = 360,
			HeightRequest = 360,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical),
			ItemsSource = Enumerable.Range(1, 12).Select(index => $"Touch row {index}").ToList(),
			ItemTemplate = new DataTemplate(CreateTouchClaimRow)
		};

		return new Grid
		{
			Children =
			{
				collectionView
			}
		};
	}

	View CreateTouchClaimRow()
	{
		var contentLabel = new Label
		{
			AutomationId = "Issue7814TouchClaimRowLabel",
			FontSize = 18,
			InputTransparent = true,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		contentLabel.SetBinding(Label.TextProperty, ".");

		var touchClaimView = new Issue7814TouchClaimView
		{
			AutomationId = "Issue7814TouchClaimView",
			BackgroundColor = Colors.LightGreen,
			TouchStateChanged = state => _touchStatusLabel.Text = state
		};

		return new Grid
		{
			HeightRequest = 84,
			Margin = new Thickness(8, 4),
			BackgroundColor = Colors.LightBlue,
			Children =
			{
				touchClaimView,
				contentLabel
			}
		};
	}

	static T Row<T>(T view, int row) where T : View
	{
		Grid.SetRow(view, row);
		return view;
	}

	static T Column<T>(T view, int column) where T : View
	{
		Grid.SetColumn(view, column);
		return view;
	}

	static Label CreateOffsetLabel(string automationId, string prefix)
	{
		var label = new Label
		{
			AutomationId = automationId,
			FontSize = 12,
			Text = $"{prefix}: 0"
		};

		return label;
	}

	static void UpdateOffset(Label label, string prefix, double offset)
	{
		label.Text = $"{prefix}: {(int)Math.Round(offset)}";
	}
}

public class Issue7814TouchClaimView : View
{
	public Action<string> TouchStateChanged { get; set; }

	internal void SendTouchState(string state)
	{
		TouchStateChanged?.Invoke(state);
	}
}

#if ANDROID
public class Issue7814TouchClaimViewHandler : ViewHandler<Issue7814TouchClaimView, AView>
{
	int _moveCount;

	public Issue7814TouchClaimViewHandler() : base(ViewHandler.ViewMapper, ViewHandler.ViewCommandMapper)
	{
	}

	protected override AView CreatePlatformView()
	{
		return new AView(Context)
		{
			Clickable = true
		};
	}

	protected override void ConnectHandler(AView platformView)
	{
		base.ConnectHandler(platformView);
		platformView.Touch += OnTouch;
	}

	protected override void DisconnectHandler(AView platformView)
	{
		platformView.Touch -= OnTouch;
		base.DisconnectHandler(platformView);
	}

	void OnTouch(object sender, AView.TouchEventArgs e)
	{
		if (e.Event is null)
		{
			return;
		}

		switch (e.Event.ActionMasked)
		{
			case Android.Views.MotionEventActions.Down:
				_moveCount = 0;
				RequestTouchOwnership();
				VirtualView.SendTouchState("TouchStatus: Down");
				e.Handled = true;
				break;
			case Android.Views.MotionEventActions.Move:
				_moveCount++;
				RequestTouchOwnership();
				VirtualView.SendTouchState($"TouchStatus: Move {_moveCount}");
				e.Handled = true;
				break;
			case Android.Views.MotionEventActions.Up:
				RequestTouchOwnership();
				VirtualView.SendTouchState($"TouchStatus: Up {_moveCount}");
				e.Handled = true;
				break;
			case Android.Views.MotionEventActions.Cancel:
				VirtualView.SendTouchState($"TouchStatus: Cancel {_moveCount}");
				e.Handled = true;
				break;
		}
	}

	void RequestTouchOwnership()
	{
		PlatformView?.Parent?.RequestDisallowInterceptTouchEvent(true);
	}
}
#endif
