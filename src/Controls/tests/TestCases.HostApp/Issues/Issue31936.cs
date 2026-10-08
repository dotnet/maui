using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31936, "Back button FontImageSource glyph is not vertically centered on iOS 26", PlatformAffected.iOS)]
public class Issue31936 : Shell
{
	const string G4Route = "Issue31936G4";

	public Issue31936()
	{
		Shell.SetBackgroundColor(this, Colors.DarkSlateBlue);
		Shell.SetForegroundColor(this, Colors.White);

		var openButton = new Button
		{
			AutomationId = "Issue31936OpenG4",
			Text = "Open G4"
		};
		openButton.Clicked += async (_, _) => await GoToAsync(G4Route);

		Items.Add(new ShellContent
		{
			Title = "Commands",
			Route = "Issue31936Commands",
			Content = new ContentPage
			{
				Title = "Commands",
				Content = new VerticalStackLayout
				{
					Padding = 24,
					Spacing = 20,
					Children =
					{
						new Label
						{
							AutomationId = "Issue31936RootMarker",
							Text = "Shell BackButtonBehavior with a command and a FontImageSource icon"
						},
						openButton
					}
				}
			}
		});

		Routing.RegisterRoute(G4Route, typeof(Issue31936G4Page));
	}
}

public class Issue31936G4Page : ContentPage
{
	public Issue31936G4Page()
	{
		Title = "G4";
		BackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
		BindingContext = this;

		var backButtonBehavior = new BackButtonBehavior
		{
			IsEnabled = true,
			IsVisible = true,
			IconOverride = new FontImageSource
			{
				FontFamily = "OpenSansRegular",
				Glyph = "\u2039"
			}
		};
		backButtonBehavior.SetBinding(BackButtonBehavior.CommandProperty, nameof(BackCommand));
		Shell.SetBackButtonBehavior(this, backButtonBehavior);

		Content = new VerticalStackLayout
		{
			Padding = 24,
			Spacing = 16,
			Children =
			{
				new Label
				{
					AutomationId = "Issue31936G4Marker",
					Text = "The custom back glyph should be vertically centered in the navigation button."
				}
			}
		};
	}

	public ICommand BackCommand { get; }
}
