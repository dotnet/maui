using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31610, "ScrollView Content Misaligned in RightToLeft FlowDirection when adding views dynamically", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue31610 : TestContentPage
{
	private ScrollView _scrollView;
	private HorizontalStackLayout _stackLayout;
	private int _itemCount = 8;
	private bool _isRTL = true;

	protected override void Init()
	{
		Title = "ScrollView RTL Dynamic Content";

		var instructions = new Label
		{
			Text = "1. Scroll the ScrollView horizontally.\n2. In RTL mode, you should be able to scroll to see all items (1-8).\n3. There should be no excessive empty space on the right.\n4. Toggle FlowDirection to test both RTL and LTR behavior.",
			Margin = new Thickness(10),
			AutomationId = "InstructionsLabel"
		};

		_stackLayout = new HorizontalStackLayout
		{
			Spacing = 10,
			Padding = new Thickness(10)
		};

		for (int i = 1; i <= _itemCount; i++)
		{
			AddItem(i);
		}

		_scrollView = new ScrollView
		{
			Content = _stackLayout,
			Orientation = ScrollOrientation.Horizontal,
			FlowDirection = FlowDirection.RightToLeft,
			BackgroundColor = Colors.LightGray,
			HeightRequest = 120,
			AutomationId = "TestScrollView"
		};

		_scrollView.Scrolled += OnScrolled;

		var toggleButton = new Button
		{
			Text = "Toggle RTL/LTR",
			AutomationId = "ToggleDirectionButton",
			Margin = new Thickness(10)
		};

		toggleButton.Clicked += (s, e) =>
		{
			_isRTL = !_isRTL;
			_scrollView.FlowDirection = _isRTL ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
			toggleButton.Text = _isRTL ? "Switch to LTR" : "Switch to RTL";
		};

		var addItemsButton = new Button
		{
			Text = "Add More Items",
			AutomationId = "AddItemsButton",
			Margin = new Thickness(10)
		};

		addItemsButton.Clicked += async (s, e) =>
		{
			for (int i = 0; i < 3; i++)
			{
				AddItem(++_itemCount);
			}

			await ScrollToNewestItem();
		};

		Content = new StackLayout
		{
			Children =
			{
				instructions,
				toggleButton,
				addItemsButton,
				_scrollView
			},
			Spacing = 10
		};
	}

	private void AddItem(int index)
	{
		var border = new Border
		{
			BackgroundColor = index % 2 == 0 ? Colors.LightBlue : Colors.LightCoral,
			Stroke = Colors.DarkGray,
			StrokeThickness = 1,
			WidthRequest = 120,
			HeightRequest = 80,
			Margin = new Thickness(5),
			AutomationId = $"Item{index}"
		};

		var label = new Label
		{
			Text = $"Item {index}",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 14,
			AutomationId = $"ItemLabel{index}"
		};

		border.Content = label;
		_stackLayout.Children.Add(border);
	}

	private async Task ScrollToNewestItem()
	{
		await Task.Delay(100);

		if (_scrollView != null)
		{
			var scrollX = _isRTL ? 0 : double.MaxValue;
			await _scrollView.ScrollToAsync(scrollX, 0, true);
		}
	}

	private void OnScrolled(object sender, ScrolledEventArgs e)
	{
		if (e.ScrollX > 0 && _stackLayout.Children.Count < 20)
		{
			var shouldAdd = _stackLayout.Children.Count % 5 == 0;
			if (shouldAdd)
			{
				AddItem(++_itemCount);
			}
		}
	}
}