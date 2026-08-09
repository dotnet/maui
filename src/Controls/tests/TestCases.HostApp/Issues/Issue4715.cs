namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4715, "[Windows] Layout containers not visible to UI automation", PlatformAffected.UWP)]
public class Issue4715 : ContentPage
{
	public Issue4715()
	{
		var scrollView = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = new Thickness(16),
				Children =
				{
					new Label
					{
						Text = "Layout Automation Peer Test",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						AutomationId = "PageTitle"
					},

					// Grid with AutomationId
					new Grid
					{
						AutomationId = "TestGrid",
						BackgroundColor = Colors.LightBlue,
						HeightRequest = 60,
						Children =
						{
							new Label { Text = "Grid", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
						}
					},

					// VerticalStackLayout with AutomationId
					new VerticalStackLayout
					{
						AutomationId = "TestVerticalStackLayout",
						BackgroundColor = Colors.LightGreen,
						Children =
						{
							new Label { Text = "VerticalStackLayout", Padding = new Thickness(8) }
						}
					},

					// HorizontalStackLayout with AutomationId
					new HorizontalStackLayout
					{
						AutomationId = "TestHorizontalStackLayout",
						BackgroundColor = Colors.LightYellow,
						Children =
						{
							new Label { Text = "HorizontalStackLayout", Padding = new Thickness(8) }
						}
					},

					// FlexLayout with AutomationId
					new FlexLayout
					{
						AutomationId = "TestFlexLayout",
						BackgroundColor = Colors.LightPink,
						HeightRequest = 60,
						Children =
						{
							new Label { Text = "FlexLayout", Margin = new Thickness(8) }
						}
					},

					// AbsoluteLayout with AutomationId
					new AbsoluteLayout
					{
						AutomationId = "TestAbsoluteLayout",
						BackgroundColor = Colors.LightSteelBlue,
						HeightRequest = 60,
						Children =
						{
							new Label
							{
								Text = "AbsoluteLayout",
								Margin = new Thickness(8)
							}
						}
					},

					// Nested layout — outer has AutomationId, inner is anonymous
					new Grid
					{
						AutomationId = "TestNestedOuterGrid",
						BackgroundColor = Colors.Lavender,
						HeightRequest = 80,
						Children =
						{
							new VerticalStackLayout
							{
								// No AutomationId — anonymous inner layout
								Children =
								{
									new Label { Text = "Nested: Outer Grid (named)", HorizontalOptions = LayoutOptions.Center },
									new Label { Text = "Inner VerticalStackLayout (anonymous)", HorizontalOptions = LayoutOptions.Center, FontSize = 11 }
								}
							}
						}
					},

					// Anonymous layout — no AutomationId, should NOT be found by Appium
					new Grid
					{
						BackgroundColor = Colors.LightGray,
						HeightRequest = 60,
						Children =
						{
							new Label { Text = "Anonymous Grid (no AutomationId)", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
						}
					},

					// Sentinel label to confirm page has loaded
					new Label
					{
						Text = "All layouts rendered",
						AutomationId = "WaitForStubControl"
					}
				}
			}
		};

		Content = scrollView;
	}
}
