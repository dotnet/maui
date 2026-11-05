namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39559, "HandleOnBackPressed fails to trigger with modal open on Android", PlatformAffected.Android)]
public class Issue39559 : ContentPage
{
	public Issue39559()
	{
		var openModalButton = new Button { AutomationId = "OpenModalButton", Text = "Open modal" };
		openModalButton.Clicked += async (_, _) =>
		{
			var modalCountLabel = new Label { AutomationId = "ModalBackPressCountLabel", Text = "Modal count: 0" };
			var closeModalButton = new Button { AutomationId = "CloseModalButton", Text = "Close modal" };
			var modal = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					Children = { new Label { AutomationId = "ModalLabel", Text = "Modal" }, modalCountLabel, closeModalButton }
				}
			};

			closeModalButton.Clicked += async (_, _) => await Navigation.PopModalAsync();

#if ANDROID
			// Registered only while the modal is open and removed when it goes away, so it can't
			// consume back presses on pages shown by later tests.
			Issue39559Callback callback = null;
			if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity is AndroidX.Activity.ComponentActivity activity)
			{
				callback = new Issue39559Callback(count => modalCountLabel.Text = $"Modal count: {count}");
				activity.OnBackPressedDispatcher.AddCallback(activity, callback);
			}

			modal.Disappearing += (_, _) =>
			{
				if (callback is null)
				{
					return;
				}

				callback.Remove();
				callback.Dispose();
				callback = null;
			};
#endif
			await Navigation.PushModalAsync(modal);
		};

		Content = new VerticalStackLayout { Padding = 30, Children = { openModalButton } };
	}
}

#if ANDROID
// Registered on the Activity's dispatcher, like an app would do in MainActivity.
sealed class Issue39559Callback : AndroidX.Activity.OnBackPressedCallback
{
	readonly Action<int> _updated;
	int _count;

	public Issue39559Callback(Action<int> updated) : base(true)
	{
		_updated = updated;
	}

	public override void HandleOnBackPressed()
	{
		_count++;
		_updated(_count);
	}
}
#endif
