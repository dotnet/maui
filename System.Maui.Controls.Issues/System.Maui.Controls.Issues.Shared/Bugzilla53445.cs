using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.IsEnabled)]
#endif
	
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 53445, "Setting Grid.IsEnabled to false does not disable child controls", PlatformAffected.All)]
	public class Bugzilla53445 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout { VerticalOptions = LayoutOptions.Fill, Spacing = 20 };

			var status = new Label { Text = "Success" };

			var instructions = new Label { Text = "Disable all of the layouts by clicking the Toggle button. Then click the buttons inside each layout. If the status changes from Success to Fail, this test has failed." };

			var grid = new Grid
			{
				BackgroundColor = Color.Blue,
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "grid"
			};

			var gridButton = new Button { AutomationId = "gridbutton", Text = "Test", WidthRequest = 50 };
			grid.Children.Add(gridButton);
			gridButton.Clicked += (sender, args) => status.Text = "Fail";

			var contentView = new ContentView
			{
				BackgroundColor = Color.Green,
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "contentView"
			};

			var contentViewButton = new Button { AutomationId = "contentviewbutton", Text = "Test", WidthRequest = 50 };
			contentView.Content = contentViewButton;
			contentViewButton.Clicked += (sender, args) => status.Text = "Fail";

			var stackLayout = new StackLayout
			{
				BackgroundColor = Color.Orange,
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "stackLayout"
			};

			var stackLayoutButton = new Button { AutomationId = "stacklayoutbutton", Text = "Test", WidthRequest = 50 };
			stackLayout.Children.Add(stackLayoutButton);
			stackLayoutButton.Clicked += (sender, args) => status.Text = "Fail";

			var toggleButton = new Button { AutomationId = "toggle", Text = $"Toggle IsEnabled (currently {grid.IsEnabled})" };
			toggleButton.Clicked += (sender, args) =>
			{
				grid.IsEnabled = !grid.IsEnabled;
				contentView.IsEnabled = !contentView.IsEnabled;
				stackLayout.IsEnabled = !stackLayout.IsEnabled;
				toggleButton.Text = $"Toggle IsEnabled (currently {grid.IsEnabled})";
			};

			layout.Children.Add(instructions);
			layout.Children.Add(status);
			layout.Children.Add(toggleButton);
			layout.Children.Add(grid);
			layout.Children.Add(contentView);
			layout.Children.Add(stackLayout);

			Content = layout;
		}


#if UITEST
		[Test]
		public void Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Success"));

			// Disable the layouts
			RunningApp.Tap(q => q.Marked("toggle"));

			// Tap the grid button; the event should not fire and the label should not change
			RunningApp.Tap(q => q.Marked("gridbutton"));
			RunningApp.WaitForElement(q => q.Marked("Success"));

			// Tap the contentview button; the event should not fire and the label should not change
			RunningApp.Tap(q => q.Marked("contentviewbutton"));
			RunningApp.WaitForElement(q => q.Marked("Success"));

			// Tap the stacklayout button; the event should not fire and the label should not change
			RunningApp.Tap(q => q.Marked("stacklayoutbutton"));
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}