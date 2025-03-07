namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 26392, "Click on flyout clicks on page behind", PlatformAffected.Android)]
	public class Issue26392 : FlyoutPage
	{
		public Issue26392()
		{
			var label = new Label()
			{
				Text = "Button behind flyout was not clicked",
				AutomationId = "ActionLabel"
			};

			Flyout = new ContentPage()
			{
				Title = "Flyout",
				Content = new StackLayout()
				{
					Children =
					{
						label,
						new Button()
						{
							Margin = new Thickness(0, 50, 0, 0),
							InputTransparent = true,
							Text = "Transparent button",
							AutomationId="ButtonInFlyout"
						}
					}
				}
			};

			Detail = new NavigationPage(new ContentPage()
			{
				Title = "Main Page",
				Content = new Button()
				{
					Text = "Click me",
					Command = new Command(() => label.Text = "Button in detail was clicked"),
				}
			});

			IsPresented = true;
		}
	}
}
