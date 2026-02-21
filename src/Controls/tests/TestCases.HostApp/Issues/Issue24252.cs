namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24252, "Overlapping gesture recognizers should only fire on the topmost child view on Windows", PlatformAffected.UWP)]
public class Issue24252 : ContentPage
{
	public Issue24252()
	{
		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				CreatePanSection(),
				CreateSwipeSection(),
				CreatePinchSection()
			}
		};
	}

	static View CreatePanSection()
	{
		var statusLabel = new Label { Text = "None", AutomationId = "PanStatusLabel" };

		var parent = new Grid
		{
			BackgroundColor = Colors.LightBlue,
			WidthRequest = 250,
			HeightRequest = 150,
			HorizontalOptions = LayoutOptions.Center
		};

		var parentPan = new PanGestureRecognizer();
		parentPan.PanUpdated += (s, e) =>
		{
			if (e.StatusType == GestureStatus.Started)
			{
				statusLabel.Text = "Parent triggered";
			}
		};
		parent.GestureRecognizers.Add(parentPan);

		var child = new Image
		{
			Source = "dotnet_bot.png",
			WidthRequest = 120,
			HeightRequest = 100,
			BackgroundColor = Colors.Orange,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "PanChildBox"
		};

		var childPan = new PanGestureRecognizer();
		childPan.PanUpdated += (s, e) =>
		{
			if (e.StatusType == GestureStatus.Started)
			{
				statusLabel.Text = "Child triggered";
			}
		};
		child.GestureRecognizers.Add(childPan);

		parent.Children.Add(child);

		return new VerticalStackLayout
		{
			Spacing = 5,
			Children =
			{
				new Label { Text = "Pan: Drag on orange child", FontAttributes = FontAttributes.Bold },
				statusLabel,
				parent
			}
		};
	}

	static View CreateSwipeSection()
	{
		var statusLabel = new Label { Text = "None", AutomationId = "SwipeStatusLabel" };

		var parent = new Grid
		{
			BackgroundColor = Colors.LightGreen,
			WidthRequest = 250,
			HeightRequest = 150,
			HorizontalOptions = LayoutOptions.Center
		};

		var parentSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
		parentSwipe.Swiped += (s, e) => statusLabel.Text = "Parent triggered";
		parent.GestureRecognizers.Add(parentSwipe);

		var child = new Image
		{
			Source = "dotnet_bot.png",
			WidthRequest = 120,
			HeightRequest = 100,
			BackgroundColor = Colors.Orange,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "SwipeChildBox"
		};

		var childSwipe = new SwipeGestureRecognizer { Direction = SwipeDirection.Right };
		childSwipe.Swiped += (s, e) => statusLabel.Text = "Child triggered";
		child.GestureRecognizers.Add(childSwipe);

		parent.Children.Add(child);

		return new VerticalStackLayout
		{
			Spacing = 5,
			Children =
			{
				new Label { Text = "Swipe: Swipe right on orange child", FontAttributes = FontAttributes.Bold },
				statusLabel,
				parent
			}
		};
	}

	static View CreatePinchSection()
	{
		var statusLabel = new Label { Text = "None", AutomationId = "PinchStatusLabel" };

		var parent = new Grid
		{
			BackgroundColor = Colors.LightCoral,
			WidthRequest = 250,
			HeightRequest = 150,
			HorizontalOptions = LayoutOptions.Center
		};

		var parentPinch = new PinchGestureRecognizer();
		parentPinch.PinchUpdated += (s, e) =>
		{
			if (e.Status == GestureStatus.Started)
			{
				statusLabel.Text = "Parent triggered";
			}
		};
		parent.GestureRecognizers.Add(parentPinch);

		var child = new Image
		{
			Source = "dotnet_bot.png",
			WidthRequest = 150,
			HeightRequest = 150,
			BackgroundColor = Colors.Orange,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "PinchChildBox"
		};

		var childPinch = new PinchGestureRecognizer();
		childPinch.PinchUpdated += (s, e) =>
		{
			if (e.Status == GestureStatus.Started)
			{
				statusLabel.Text = "Child triggered";
			}
		};
		child.GestureRecognizers.Add(childPinch);

		parent.Children.Add(child);

		return new VerticalStackLayout
		{
			Spacing = 5,
			Children =
			{
				new Label { Text = "Pinch: Pinch on orange child", FontAttributes = FontAttributes.Bold },
				statusLabel,
				parent
			}
		};
	}
}