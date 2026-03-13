namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11764, "ScrollView doesn't work in the Shell Flyout Header", PlatformAffected.Android)]
	public class Issue11764 : TestShell
	{
		Label scrollPositionLabel;
		VerticalStackLayout _views;
		protected override void Init()
		{
			var sampleButton = new Button();
			sampleButton.Text = "Click to Scroll to Bottom";
			sampleButton.FontSize = 16;
			sampleButton.AutomationId = "SampleButton";

			scrollPositionLabel = new Label
			{
				Text = "ScrollView - Y position is 0",
				FontSize = 18,
				TextColor = Colors.White,
				BackgroundColor = Colors.Green,
				Padding = 10
			};
			var scrollView = new ScrollView
			{
				HeightRequest = 200,
				AutomationId= "FlyoutScrollView",
				Content = new VerticalStackLayout
				{
					Spacing = 10,
					Padding = 20,
					Children =
					{
						sampleButton,
						new Label { Text = "Item 2", FontSize = 16, TextColor = Colors.White },
						new BoxView { HeightRequest = 100, Color = Colors.Blue },
						new Label { Text = "Item 3", FontSize = 16, TextColor = Colors.White },
						new BoxView { HeightRequest = 100, Color = Colors.Orange },
						new Label { Text = "Item 4", FontSize = 16, TextColor = Colors.White },
						new BoxView { HeightRequest = 100, Color = Colors.Purple },
						new Label { Text = "Item 5", FontSize = 16, TextColor = Colors.White },
						new BoxView { HeightRequest = 100, Color = Colors.Cyan },
						new Label
						{
							Text = "Bottom of ScrollView - This is the last item",
							FontSize = 18,
							TextColor = Colors.White,
							BackgroundColor = Colors.DarkGreen,
							Padding = 10
						}
					}
				}
			};
			scrollView.Scrolled += scrollView_Scrolled;
			_views = new VerticalStackLayout
			{
				Children =
				{
					scrollView,
					scrollPositionLabel
				}
			};
			FlyoutHeader = _views;
			AddFlyoutItem(CreatePage("Page 1"), "Page 1");
		}

		private void scrollView_Scrolled(object sender, ScrolledEventArgs e)
		{
			scrollPositionLabel.Text = "The ScrollView is scrolls vertically";
		}
		ContentPage CreatePage(string title)
		{
            var buttonText = new Button();
			buttonText.Text = "Open Flyout";
			buttonText.AutomationId = "OpenFlyoutButton";
            buttonText.Clicked += (sender, e) => Shell.Current.FlyoutIsPresented = true;
			return new ContentPage
			{
				Title = title,
				Content = new VerticalStackLayout
				{
					Padding = 20,
					Children =
					{
						buttonText,
                    }
				}
			};
		}
	}
}
