namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "LongPress Gesture Interaction Tests", PlatformAffected.All)]
public class LongPressGestureInteraction : ContentPage
{
	int _tapCount;
	int _longPressCount;
	int _longPress2Count;
	int _longPress3Count;
	int _longPress4Count;
	int _longPress5Count;
	int _swipeCount;

	readonly Label _tapLabel;
	readonly Label _longPressLabel;
	readonly Label _swipeLabel;
	readonly Label _longPress2Label;
	readonly Label _longPress3Label;
	readonly Label _longPress4Label;
	readonly Label _longPress5Label;

	public LongPressGestureInteraction()
	{
		Title = "LongPress Gesture Interaction Tests";

		_tapLabel = new Label { AutomationId = "TapLabel", Text = "Tap Count: 0" };
		_longPressLabel = new Label { AutomationId = "LongPressLabel", Text = "Long Press Count: 0" };
		_swipeLabel = new Label { AutomationId = "SwipeLabel", Text = "Swipe Count: 0" };
		_longPress2Label = new Label { AutomationId = "LongPress2Label", Text = "Long Press Count: 0" };
		_longPress3Label = new Label { AutomationId = "LongPress3Label", Text = "Long Press Count: 0" };
		_longPress4Label = new Label { AutomationId = "LongPress4Label", Text = "LongPress1 Count: 0" };
		_longPress5Label = new Label { AutomationId = "LongPress5Label", Text = "LongPress2 Count: 0" };

		// Test 1: LongPress + Tap
		var tapAndLongPressBorder = new Border
		{
			AutomationId = "TapAndLongPressFrame",
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 50,
			Padding = 4,
			Content = new Label
			{
				Text = "Tap or Long Press Me",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		tapAndLongPressBorder.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() =>
			{
				_tapCount++;
				_tapLabel.Text = $"Tap Count: {_tapCount}";
			})
		});
		tapAndLongPressBorder.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPressCount++;
				_longPressLabel.Text = $"Long Press Count: {_longPressCount}";
			})
		});

		// Test 2: LongPress + Swipe
		var swipeAndLongPressBorder = new Border
		{
			AutomationId = "SwipeAndLongPressFrame",
			BackgroundColor = Colors.LightGreen,
			HeightRequest = 50,
			Padding = 4,
			Content = new Label
			{
				Text = "Swipe Left or Long Press",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		var swipeGesture = new SwipeGestureRecognizer { Direction = SwipeDirection.Left };
		swipeGesture.Swiped += (s, e) =>
		{
			_swipeCount++;
			_swipeLabel.Text = $"Swipe Count: {_swipeCount}";
		};
		swipeAndLongPressBorder.GestureRecognizers.Add(swipeGesture);
		swipeAndLongPressBorder.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress2Count++;
				_longPress2Label.Text = $"Long Press Count: {_longPress2Count}";
			})
		});

		// Test 3: LongPress in ScrollView
		var longPressInScrollBorder = new Border
		{
			AutomationId = "LongPressInScrollFrame",
			BackgroundColor = Colors.Orange,
			HeightRequest = 50,
			Padding = 4,
			Content = new Label
			{
				Text = "Long Press in ScrollView",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		longPressInScrollBorder.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress3Count++;
				_longPress3Label.Text = $"Long Press Count: {_longPress3Count}";
			})
		});

		// Test 4: Multiple LongPress recognizers
		var longPress1Border = new Border
		{
			AutomationId = "LongPress1",
			BackgroundColor = Colors.Pink,
			WidthRequest = 150,
			HeightRequest = 50,
			Padding = 4,
			Content = new Label
			{
				Text = "Long Press 1",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		longPress1Border.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress4Count++;
				_longPress4Label.Text = $"LongPress1 Count: {_longPress4Count}";
			})
		});

		var longPress2Border = new Border
		{
			AutomationId = "LongPress2",
			BackgroundColor = Colors.LightCoral,
			WidthRequest = 150,
			HeightRequest = 50,
			Padding = 4,
			Content = new Label
			{
				Text = "Long Press 2",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		longPress2Border.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress5Count++;
				_longPress5Label.Text = $"LongPress2 Count: {_longPress5Count}";
			})
		});

		// Use compact layout so all test areas fit on screen without scrolling
		Content = new VerticalStackLayout
		{
			Spacing = 6,
			Padding = 8,
			Children =
			{
				tapAndLongPressBorder,
				_tapLabel,
				_longPressLabel,

				swipeAndLongPressBorder,
				_swipeLabel,
				_longPress2Label,

				new ScrollView
				{
					AutomationId = "TestScrollView",
					HeightRequest = 80,
					BackgroundColor = Colors.LightYellow,
					Content = new VerticalStackLayout
					{
						Children =
						{
							longPressInScrollBorder,
							new BoxView { HeightRequest = 200, Color = Colors.Gray }
						}
					}
				},
				_longPress3Label,

				new HorizontalStackLayout
				{
					Spacing = 10,
					Children = { longPress1Border, longPress2Border }
				},
				_longPress4Label,
				_longPress5Label,
			}
		};
	}
}
