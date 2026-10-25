namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39559, "HandleOnBackPressed fails to trigger with modal open on Android", PlatformAffected.Android)]
public class Issue39559 : ContentPage
{
#if ANDROID
	static int s_backPressCount;
	static bool s_callbackRegistered;
#endif

	public Issue39559()
	{
		var countLabel = new Label { AutomationId = "BackPressCountLabel", Text = "Count: 0" };

		var openModalButton = new Button { AutomationId = "OpenModalButton", Text = "Open modal" };
		openModalButton.Clicked += async (_, _) =>
		{
			var modalCountLabel = new Label { AutomationId = "ModalBackPressCountLabel", Text = "Modal count: 0" };
			var modalCloseButton = new Button { AutomationId = "CloseModalButton", Text = "Close modal" };
			var modal = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Children = { new Label { AutomationId = "ModalLabel", Text = "Modal" }, modalCountLabel, modalCloseButton }
				}
			};

			modalCloseButton.Clicked += async (_, _) => await Navigation.PopModalAsync();
#if ANDROID
			// Keep the label in sync so the test can read how many times the Activity callback ran.
			modal.Appearing += (_, _) => modalCountLabel.Text = $"Modal count: {s_backPressCount}";
			Issue39559Callback.Updated = count => Dispatcher.Dispatch(() => modalCountLabel.Text = $"Modal count: {count}");
#endif
			await Navigation.PushModalAsync(modal);
		};

#if ANDROID
		if (!s_callbackRegistered && Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is AndroidX.Activity.ComponentActivity activity)
		{
			s_callbackRegistered = true;
			activity.OnBackPressedDispatcher.AddCallback(activity, new Issue39559Callback(() =>
			{
				s_backPressCount++;
				return s_backPressCount;
			}));
		}
#endif

		Content = new VerticalStackLayout { Padding = 30, Children = { countLabel, openModalButton } };
	}
}

#if ANDROID
// Registered on the Activity's dispatcher, like an app would do in MainActivity.
// It does not consume anything beyond recording the call; the modal is closed by the test via the close button.
sealed class Issue39559Callback : AndroidX.Activity.OnBackPressedCallback
{
	readonly Func<int> _onBack;
	public static Action<int> Updated;

	public Issue39559Callback(Func<int> onBack) : base(true)
	{
		_onBack = onBack;
	}

	public override void HandleOnBackPressed() => Updated?.Invoke(_onBack());
}
#endif
