namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34073, "OnNavigatingFrom is reporting wrong DestinationPage", PlatformAffected.All)]
public class Issue34073 : TestShell
{
	protected override void Init()
	{
		Routing.RegisterRoute(nameof(Issue34073PageB), typeof(Issue34073PageB));
		AddContentPage(new Issue34073PageA());
	}

	public class Issue34073PageA : ContentPage
	{
		readonly Label _navigatingFromResult;

		public Issue34073PageA()
		{
			_navigatingFromResult = new Label
			{
				Text = "Waiting...",
				AutomationId = "NavigatingFromResult"
			};

			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = new Thickness(20),
				Children =
				{
					new Label { Text = "Page A" },
					new Button
					{
						Text = "Navigate to Page B",
						AutomationId = "NavigateButton",
						Command = new Command(async () =>
							await Shell.Current.GoToAsync(nameof(Issue34073PageB)))
					},
					new Label { Text = "OnNavigatingFrom DestinationPage:" },
					_navigatingFromResult
				}
			};
		}

		protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
		{
			base.OnNavigatingFrom(args);
			_navigatingFromResult.Text = args.DestinationPage?.GetType().Name ?? "null";
		}
	}

	public class Issue34073PageB : ContentPage
	{
		public Issue34073PageB()
		{
			Content = new VerticalStackLayout
			{
				Spacing = 10,
				Padding = new Thickness(20),
				Children =
				{
					new Label { Text = "Page B" },
					new Button
					{
						Text = "Go Back",
						AutomationId = "GoBackButton",
						Command = new Command(async () =>
							await Shell.Current.GoToAsync(".."))
					}
				}
			};
		}
	}
}
