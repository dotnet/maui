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
		var tapAndLongPressFrame = new Frame
		{
			AutomationId = "TapAndLongPressFrame",
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 100,
			Content = new Label
			{
				Text = "Tap or Long Press Me",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		tapAndLongPressFrame.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() =>
			{
				_tapCount++;
				_tapLabel.Text = $"Tap Count: {_tapCount}";
			})
		});
		tapAndLongPressFrame.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPressCount++;
				_longPressLabel.Text = $"Long Press Count: {_longPressCount}";
			})
		});

		// Test 2: LongPress + Swipe
		var swipeAndLongPressFrame = new Frame
		{
			AutomationId = "SwipeAndLongPressFrame",
			BackgroundColor = Colors.LightGreen,
			HeightRequest = 100,
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
		swipeAndLongPressFrame.GestureRecognizers.Add(swipeGesture);
		swipeAndLongPressFrame.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress2Count++;
				_longPress2Label.Text = $"Long Press Count: {_longPress2Count}";
			})
		});

		// Test 3: LongPress in ScrollView
		var longPressInScrollFrame = new Frame
		{
			AutomationId = "LongPressInScrollFrame",
			Margin = 10,
			BackgroundColor = Colors.Orange,
			HeightRequest = 100,
			Content = new Label
			{
				Text = "Long Press in ScrollView",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		longPressInScrollFrame.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress3Count++;
				_longPress3Label.Text = $"Long Press Count: {_longPress3Count}";
			})
		});

		// Test 4: Multiple LongPress recognizers
		var longPress1Frame = new Frame
		{
			AutomationId = "LongPress1",
			BackgroundColor = Colors.Pink,
			WidthRequest = 150,
			HeightRequest = 100,
			Content = new Label
			{
				Text = "Long Press 1",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		longPress1Frame.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress4Count++;
				_longPress4Label.Text = $"LongPress1 Count: {_longPress4Count}";
			})
		});

		var longPress2Frame = new Frame
		{
			AutomationId = "LongPress2",
			BackgroundColor = Colors.LightCoral,
			WidthRequest = 150,
			HeightRequest = 100,
			Content = new Label
			{
				Text = "Long Press 2",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};
		longPress2Frame.GestureRecognizers.Add(new LongPressGestureRecognizer
		{
			Command = new Command(() =>
			{
				_longPress5Count++;
				_longPress5Label.Text = $"LongPress2 Count: {_longPress5Count}";
			})
		});

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label { Text = "Test 1: LongPress + Tap", FontAttributes = FontAttributes.Bold },
					tapAndLongPressFrame,
					_tapLabel,
					_longPressLabel,

					new Label { Text = "Test 2: LongPress + Swipe", FontAttributes = FontAttributes.Bold },
					swipeAndLongPressFrame,
					_swipeLabel,
					_longPress2Label,

					new Label { Text = "Test 3: LongPress in ScrollView", FontAttributes = FontAttributes.Bold },
					new ScrollView
					{
						AutomationId = "TestScrollView",
						HeightRequest = 200,
						BackgroundColor = Colors.LightYellow,
						Content = new VerticalStackLayout
						{
							Children =
							{
								longPressInScrollFrame,
								new BoxView { HeightRequest = 400, Color = Colors.Gray }
							}
						}
					},
					_longPress3Label,

					new Label { Text = "Test 4: Multiple LongPress", FontAttributes = FontAttributes.Bold },
					new HorizontalStackLayout
					{
						Spacing = 10,
						Children = { longPress1Frame, longPress2Frame }
					},
					_longPress4Label,
					_longPress5Label,
				}
			}
		};
	}
}
