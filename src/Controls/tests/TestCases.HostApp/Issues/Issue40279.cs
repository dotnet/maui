#if ANDROID
using AndroidX.Fragment.App;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40279, "Replacing Shell while stopped does not crash on resume", PlatformAffected.Android)]
public class Issue40279 : Shell
{
	public Issue40279() : this(false)
	{
	}

	Issue40279(bool signedIn)
	{
		var page = new ContentPage
		{
			Title = signedIn ? "Signed in" : "Sign in"
		};

		if (signedIn)
		{
			page.Content = new Label
			{
				AutomationId = "Issue40279SignedInLabel",
				Text = "Shell replaced while stopped"
			};
		}
		else
		{
			var button = new Button
			{
				AutomationId = "Issue40279StartSignInButton",
				Text = "Start sign-in, then background the app"
			};

			button.Clicked += (_, _) =>
			{
				var window = Window ?? throw new InvalidOperationException("The Shell must be attached to a window.");
				button.IsEnabled = false;
				window.Stopped += OnWindowStopped;

				void OnWindowStopped(object sender, EventArgs e)
				{
					window.Stopped -= OnWindowStopped;

					// Queue the replacement after the stopped lifecycle callback completes.
					if (!window.Dispatcher.Dispatch(() =>
					{
#if ANDROID
						if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is not FragmentActivity activity ||
							!activity.SupportFragmentManager.IsStateSaved)
						{
							throw new InvalidOperationException("Fragment state must be saved before replacing the Shell.");
						}
#endif

						window.Page = new Issue40279(true);
					}))
					{
						throw new InvalidOperationException("Could not dispatch the Shell replacement.");
					}
				}
			};

			page.Content = button;
		}

		Items.Add(new ShellContent
		{
			Title = page.Title,
			Content = page
		});
	}
}
