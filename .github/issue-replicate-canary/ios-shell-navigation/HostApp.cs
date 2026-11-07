#if IOS
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 37360, "Shell root remains connected after reentrant modal navigation", PlatformAffected.iOS)]
public class Issue37360 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		Routing.RegisterRoute("Issue37360Modal", typeof(Issue37360Modal));
		Routing.RegisterRoute("Issue37360Detail", typeof(Issue37360Detail));

		var phase = new Label { AutomationId = "SequencePhase", Text = "Ready", FontSize = 12 };
		var state = new Label { AutomationId = "RootState", Text = "Root: waiting for handler", FontSize = 12 };
		Shell.SetTitleView(this, new VerticalStackLayout { Children = { phase, state } });

		var root = new ContentPage { Title = "Root", BackgroundColor = Colors.SeaGreen };
		var unexpectedPops = 0;
		void UpdateState()
		{
			state.Text = $"Root: {(root.Handler is null ? "disconnected" : "connected")}; unexpected pops: {unexpectedPops}";
		}

		root.HandlerChanged += (_, _) => UpdateState();
		root.NavigatedFrom += (_, args) =>
		{
			if (args.NavigationType is NavigationType.Pop or NavigationType.PopToRoot)
				unexpectedPops++;
			UpdateState();
		};

		async Task RunControl()
		{
			phase.Text = "Control running";
			await GoToAsync("Issue37360Detail", false);
			await Task.Delay(700);
			await GoToAsync("..", false);
			await Task.Delay(700);
			UpdateState();
			phase.Text = "Control complete";
		}

		async Task RunRepro()
		{
			for (var cycle = 1; cycle <= 3; cycle++)
			{
				phase.Text = $"Modal sequence {cycle}";
				await GoToAsync("Issue37360Modal", false);
				await Task.Delay(700);
				var dismissed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				Task detailNavigation = Task.CompletedTask;
				void OnDismissed(object sender, ShellNavigatedEventArgs args)
				{
					if (CurrentPage != root || args.Source is not (ShellNavigationSource.Pop or ShellNavigationSource.PopToRoot))
						return;
					Navigated -= OnDismissed;
					// Start the push inside the pop's event, before Shell finishes notifying the previous page.
					detailNavigation = GoToAsync("Issue37360Detail", false);
					dismissed.SetResult();
				}

				Navigated += OnDismissed;
				try
				{
					await GoToAsync("..", false);
					await dismissed.Task.WaitAsync(TimeSpan.FromSeconds(5));
					await detailNavigation;
				}
				finally
				{
					Navigated -= OnDismissed;
				}

				await Task.Delay(700);
				await GoToAsync("..", false);
				await Task.Delay(700);
				UpdateState();
				if (root.Handler is null || unexpectedPops > 0)
					break;
			}
			phase.Text = "Repro complete";
		}

		root.Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 16,
			Children =
			{
				new Label { Text = "Shell root page", AutomationId = "RootContent", FontSize = 24, TextColor = Colors.White },
				new Button { Text = "Normal navigation control", AutomationId = "RunControl", Command = new Command(async () => await RunControl()) },
				new Button { Text = "Run modal-dismissal sequence", AutomationId = "RunRepro", Command = new Command(async () => await RunRepro()) }
			}
		};
		AddContentPage(root, "Issue37360Root");
	}
}

public class Issue37360Modal : ContentPage
{
	public Issue37360Modal()
	{
		Shell.SetPresentationMode(this, PresentationMode.Modal);
		Title = "Modal";
		BackgroundColor = Colors.CornflowerBlue;
		Content = new Label { Text = "Modal page", FontSize = 24, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
	}
}

public class Issue37360Detail : ContentPage
{
	public Issue37360Detail()
	{
		Title = "Detail";
		BackgroundColor = Colors.Goldenrod;
		Content = new Label { Text = "Detail page", FontSize = 24, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
	}
}
#endif
