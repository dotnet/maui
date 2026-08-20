namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4715, "[Windows] Layout containers not visible to UI automation", PlatformAffected.UWP)]
public class Issue4715 : ContentPage
{
	public Issue4715()
	{
		// Grid with AutomationId only. This verifies UI tests can find layout containers
		// by AutomationId without needing explicit accessible-tree opt-in.
		var testGrid = new Grid
		{
			AutomationId = "TestGrid",
			BackgroundColor = Colors.LightBlue,
			HeightRequest = 60,
			Children =
			{
				new Label { Text = "Grid", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
			}
		};

		// VerticalStackLayout with AutomationId and explicit accessible-tree opt-in.
		var testVerticalStackLayout = new VerticalStackLayout
		{
			AutomationId = "TestVerticalStackLayout",
			BackgroundColor = Colors.LightGreen,
			Children =
			{
				new Label { Text = "VerticalStackLayout", Padding = new Thickness(8) }
			}
		};
		SemanticProperties.SetDescription(testVerticalStackLayout, "Test vertical stack layout");
		AutomationProperties.SetIsInAccessibleTree(testVerticalStackLayout, true);

		// HorizontalStackLayout with AutomationId and explicit accessible-tree opt-in.
		var testHorizontalStackLayout = new HorizontalStackLayout
		{
			AutomationId = "TestHorizontalStackLayout",
			BackgroundColor = Colors.LightYellow,
			Children =
			{
				new Label { Text = "HorizontalStackLayout", Padding = new Thickness(8) }
			}
		};
		SemanticProperties.SetDescription(testHorizontalStackLayout, "Test horizontal stack layout");
		AutomationProperties.SetIsInAccessibleTree(testHorizontalStackLayout, true);

		// FlexLayout with AutomationId and explicit accessible-tree opt-in.
		var testFlexLayout = new FlexLayout
		{
			AutomationId = "TestFlexLayout",
			BackgroundColor = Colors.LightPink,
			HeightRequest = 60,
			Children =
			{
				new Label { Text = "FlexLayout", Margin = new Thickness(8) }
			}
		};
		SemanticProperties.SetDescription(testFlexLayout, "Test flex layout");
		AutomationProperties.SetIsInAccessibleTree(testFlexLayout, true);

		// AbsoluteLayout with AutomationId and explicit accessible-tree opt-in.
		var testAbsoluteLayout = new AbsoluteLayout
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
		};
		SemanticProperties.SetDescription(testAbsoluteLayout, "Test absolute layout");
		AutomationProperties.SetIsInAccessibleTree(testAbsoluteLayout, true);

		// Nested layout — outer has AutomationId and explicit accessible-tree opt-in, inner is anonymous.
		var testNestedOuterGrid = new Grid
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
		};
		SemanticProperties.SetDescription(testNestedOuterGrid, "Test nested outer grid layout");
		AutomationProperties.SetIsInAccessibleTree(testNestedOuterGrid, true);

		// Layout with an AutomationId but an explicit accessible-tree opt-out (IsInAccessibleTree="False").
		// The Raw opt-out removes it from the UIA Control view, so Appium must NOT be able to find it by
		// its AutomationId — proving the explicit opt-out takes precedence over the AutomationId test hook.
		var testOptedOutGrid = new Grid
		{
			AutomationId = "OptedOutGrid",
			BackgroundColor = Colors.LightGray,
			HeightRequest = 60,
			Children =
			{
				new Label { Text = "Opted-out Grid (IsInAccessibleTree=False)", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center }
			}
		};
		AutomationProperties.SetIsInAccessibleTree(testOptedOutGrid, false);

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
					testGrid,
					testVerticalStackLayout,
					testHorizontalStackLayout,
					testFlexLayout,
					testAbsoluteLayout,
					testNestedOuterGrid,
					testOptedOutGrid,

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
