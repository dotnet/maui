using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29312, "CarouselView2 position label and IndicatorView do not update when navigating", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue29312 : TestContentPage
{
	private CarouselView2 _carouselView;
	private Label _positionLabel;
	private Label _debugLabel;
	private IndicatorView _indicatorView;
	private ObservableCollection<Issue29312CarouselItem> _items;
	private int _currentPosition = 0;

	protected override void Init()
	{
		Title = "CarouselView2 Position Update - Testing Issue 29312";

		_items = new ObservableCollection<Issue29312CarouselItem>();
		for (int i = 1; i <= 5; i++)
		{
			_items.Add(new Issue29312CarouselItem
			{
				Text = $"Item {i}",
				BackgroundColor = i % 2 == 0 ? Colors.LightBlue : Colors.LightCoral,
				Index = i
			});
		}

		var instructions = new Label
		{
			Text = "TESTING CarouselView2 Handler Issue 29312:\n1. Click 'Next' button\n2. Position label should update (e.g., '1/5' to '2/5')\n3. IndicatorView should highlight the current item\n4. If using CarouselViewHandler2, position will NOT update during swipe\n5. Test both directions with Previous/Next",
			Margin = new Thickness(10),
			AutomationId = "InstructionsLabel"
		};

		_positionLabel = new Label
		{
			Text = GetPositionText(),
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 18,
			FontAttributes = FontAttributes.Bold,
			AutomationId = "PositionLabel",
			Margin = new Thickness(10)
		};

		_carouselView = new CarouselView2
		{
			ItemsSource = _items,
			HeightRequest = 200,
			AutomationId = "TestCarouselView",
			ItemTemplate = new DataTemplate(() =>
			{
				var border = new Border
				{
					StrokeThickness = 2,
					Stroke = Colors.DarkGray,
					Margin = new Thickness(10),
					BackgroundColor = Colors.Transparent
				};

				var stackLayout = new StackLayout
				{
					Padding = new Thickness(20),
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				};

				var itemLabel = new Label
				{
					FontSize = 24,
					FontAttributes = FontAttributes.Bold,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				itemLabel.SetBinding(Label.TextProperty, "Text");

				var indexLabel = new Label
				{
					FontSize = 16,
					HorizontalOptions = LayoutOptions.Center
				};
				indexLabel.SetBinding(Label.TextProperty, new Binding("Index", stringFormat: "Index: {0}"));

				stackLayout.Children.Add(itemLabel);
				stackLayout.Children.Add(indexLabel);

				border.Content = stackLayout;
				border.SetBinding(Border.BackgroundColorProperty, "BackgroundColor");

				return border;
			})
		};

		_indicatorView = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "TestIndicatorView",
			IndicatorColor = Colors.Gray,
			SelectedIndicatorColor = Colors.Blue,
			IndicatorSize = 10
		};

		_indicatorView.SetBinding(IndicatorView.ItemsSourceProperty, new Binding("ItemsSource", source: _carouselView));
		_indicatorView.SetBinding(IndicatorView.PositionProperty, new Binding("Position", source: _carouselView));

		_carouselView.PositionChanged += OnPositionChanged;
		_carouselView.CurrentItemChanged += OnCurrentItemChanged;

		var previousButton = new Button
		{
			Text = "Previous",
			AutomationId = "PreviousButton",
			Margin = new Thickness(5)
		};
		previousButton.Clicked += OnPreviousClicked;

		var nextButton = new Button
		{
			Text = "Next",
			AutomationId = "NextButton",
			Margin = new Thickness(5)
		};
		nextButton.Clicked += OnNextClicked;

		var buttonLayout = new StackLayout
		{
			Orientation = StackOrientation.Horizontal,
			HorizontalOptions = LayoutOptions.Center,
			Children = { previousButton, nextButton }
		};

		var resetButton = new Button
		{
			Text = "Reset to Position 0",
			AutomationId = "ResetButton",
			Margin = new Thickness(10)
		};
		resetButton.Clicked += OnResetClicked;

		var currentItemButton = new Button
		{
			Text = "Set Current Item to Item 3",
			AutomationId = "SetCurrentItemButton",
			Margin = new Thickness(10)
		};
		currentItemButton.Clicked += OnSetCurrentItemClicked;

		_debugLabel = new Label
		{
			Text = "Debug Info: Position=0, CurrentItem=Item 1",
			FontSize = 12,
			AutomationId = "DebugLabel",
			Margin = new Thickness(10)
		};

		var handlerInfoLabel = new Label
		{
			Text = $"Handler: {_carouselView.GetType().Name} -> Expected: CarouselViewHandler2 for bug reproduction",
			FontSize = 10,
			AutomationId = "HandlerInfoLabel",
			Margin = new Thickness(10),
			TextColor = Colors.Orange
		};

		Content = new StackLayout
		{
			Children =
			{
				instructions,
				handlerInfoLabel,
				_positionLabel,
				_carouselView,
				_indicatorView,
				buttonLayout,
				resetButton,
				currentItemButton,
				_debugLabel
			},
			Spacing = 10
		};

		UpdateDebugInfo();
	}

	private void OnPositionChanged(object sender, PositionChangedEventArgs e)
	{
		_currentPosition = e.CurrentPosition;
		_positionLabel.Text = GetPositionText();
		UpdateDebugInfo();
	}

	private void OnCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
	{
		if (e.CurrentItem is Issue29312CarouselItem item)
		{
			UpdateDebugInfo();
		}
	}

	private void OnPreviousClicked(object sender, EventArgs e)
	{
		if (_carouselView.Position > 0)
		{
			_carouselView.Position--;
		}
	}

	private void OnNextClicked(object sender, EventArgs e)
	{
		if (_carouselView.Position < _items.Count - 1)
		{
			_carouselView.Position++;
		}
	}

	private void OnResetClicked(object sender, EventArgs e)
	{
		_carouselView.Position = 0;
	}

	private void OnSetCurrentItemClicked(object sender, EventArgs e)
	{
		if (_items.Count >= 3)
		{
			_carouselView.CurrentItem = _items[2]; // Item 3 (index 2)
		}
	}

	private string GetPositionText()
	{
		return $"{_currentPosition + 1}/{_items.Count}";
	}

	private void UpdateDebugInfo()
	{
		if (_debugLabel != null && _carouselView != null)
		{
			var currentItem = _carouselView.CurrentItem as Issue29312CarouselItem;
			var currentItemText = currentItem?.Text ?? "null";
			_debugLabel.Text = $"Debug Info: Position={_carouselView.Position}, CurrentItem={currentItemText}";
		}
	}

	public class Issue29312CarouselItem
	{
		public string Text { get; set; }
		public Color BackgroundColor { get; set; }
		public int Index { get; set; }
	}
}