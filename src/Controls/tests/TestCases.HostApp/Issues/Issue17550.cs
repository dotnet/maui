namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17550, "Changing Shell.NavBarIsVisible does not update the nav bar on Mac / iOS",
	PlatformAffected.UWP)]
public class Issue17550 : TestShell
{
	Button _toggleButton;

	protected override void Init()
	{
		// Create a toggle button similar to the one in SandboxShell.xaml
		_toggleButton = new Button
		{
			Text = Shell.GetNavBarIsVisible(this) ? "Hide NavBar" : "Show NavBar",
			HeightRequest = 50,
			WidthRequest = 150,
			AutomationId = "NavBarToggleButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		_toggleButton.Clicked += OnNavBarToggleButtonClicked;

		// Add the button to a content page in the shell
		Items.Add(new ShellContent
		{
			Content = new ContentPage
			{
				Title = "Home",
				Content = new StackLayout
				{
					Children =
						{
							new Label
							{
								Text = "Toggle NavBar visibility",
								HorizontalOptions = LayoutOptions.Center
							},
							_toggleButton
						},
					VerticalOptions = LayoutOptions.Center
				}
			}
		});
	}

	void OnNavBarToggleButtonClicked(object sender, EventArgs e)
	{
		// Toggle the NavBar visibility
		bool isCurrentlyVisible = Shell.GetNavBarIsVisible(this);
		bool newVisibility = !isCurrentlyVisible;
		Shell.SetNavBarIsVisible(this, newVisibility);

		// Update the button text to reflect the new state
		_toggleButton.Text = newVisibility ? "Hide NavBar" : "Show NavBar";
	}
}
