namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35890,
	"HideSoftInputOnTapped should be evaluated per-page when pushing through a NavigationPage",
	PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue35890NavigationPage : NavigationPage
{
	public Issue35890NavigationPage() : base(new PageA35890())
	{
	}

	class PageA35890 : ContentPage
	{
		public PageA35890()
		{
			HideSoftInputOnTapped = true;
			Title = "Page A";

			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "Page A — HideSoftInputOnTapped = True",
						AutomationId = "PageALabel"
					},
					new Entry
					{
						Placeholder = "Username",
						AutomationId = "PageAEntry"
					},
					new Button
					{
						Text = "Push Page B",
						AutomationId = "PushPageBButton",
						Command = new Command(async () => await Navigation.PushAsync(new PageB35890()))
					}
				}
			};
		}
	}

	class PageB35890 : ContentPage
	{
		public PageB35890()
		{
			// HideSoftInputOnTapped is not set here, so it defaults to false.
			// Per-page semantics require that Page B is not affected by Page A's setting.
			Title = "Page B";

			var resultLabel = new Label
			{
				AutomationId = "ResultLabel",
				Text = ""
			};

			Content = new VerticalStackLayout
			{
				Spacing = 20,
				Padding = 20,
				Children =
				{
					new Label
					{
						Text = "Page B — HideSoftInputOnTapped = False (default)",
						AutomationId = "PageBLabel"
					},
					new Entry
					{
						Placeholder = "Tap to show keyboard",
						AutomationId = "PageBEntry"
					},
					new Button
					{
						Text = "Do Something",
						AutomationId = "PageBButton",
						Command = new Command(() => resultLabel.Text = "Button Tapped")
					},
					resultLabel
				}
			};
		}
	}
}
