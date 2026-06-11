using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35826, "PickPhotosAsync hangs when called from a child activity", PlatformAffected.Android)]
public class Issue35826 : ContentPage
{
	public Issue35826()
	{
		var instructions = new Label
		{
			AutomationId = "InstructionsLabel",
			Text = "1. Tap 'Open Child Activity'\n" +
			       "2. In the child activity, tap 'Pick Photos'\n" +
			       "3. Press Back to cancel the picker\n" +
			       "Expected: Status shows 'Cancelled'\n" +
			       "Bug: Picker hangs indefinitely",
			FontSize = 14
		};

		var openButton = new Button
		{
			AutomationId = "OpenChildActivityButton",
			Text = "Open Child Activity",
			HorizontalOptions = LayoutOptions.Fill
		};
		openButton.Clicked += OnOpenChildActivityClicked;

		var statusLabel = new Label
		{
			AutomationId = "StatusLabel",
			Text = "Status: Ready",
			FontSize = 16,
			FontAttributes = FontAttributes.Bold
		};

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 25,
			Children = { instructions, openButton, statusLabel }
		};
	}

	void OnOpenChildActivityClicked(object sender, EventArgs e)
	{
#if ANDROID
		var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
		if (activity != null)
		{
			var intent = new Android.Content.Intent(activity, typeof(Issue35826ChildActivity));
			activity.StartActivity(intent);
		}
#endif
	}
}

#if ANDROID
// A plain AppCompatActivity that calls MediaPicker.PickPhotosAsync().
// Before the fix, Platform.Init() on this activity was silently ignored by the guard in
// ActivityForResultRequest.Register(), so no launcher was registered for it and the
// picker task never completed. After the fix each activity gets its own launcher entry
// in the ConditionalWeakTable, so the result is delivered correctly.
[Android.App.Activity(Label = "Issue35826 Child Activity", Theme = "@style/Maui.SplashTheme")]
public class Issue35826ChildActivity : AndroidX.AppCompat.App.AppCompatActivity
{
	Android.Widget.TextView _resultLabel;

	protected override void OnCreate(Android.OS.Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		Microsoft.Maui.ApplicationModel.Platform.Init(this, savedInstanceState);

		var layout = new Android.Widget.LinearLayout(this)
		{
			Orientation = Android.Widget.Orientation.Vertical
		};
		layout.SetPadding(50, 50, 50, 50);

		_resultLabel = new Android.Widget.TextView(this)
		{
			Text = "Result: Ready",
			ContentDescription = "ChildActivityResultLabel"
		};
		_resultLabel.SetPadding(0, 0, 0, 50);

		var pickButton = new Android.Widget.Button(this)
		{
			Text = "Pick Photos",
			ContentDescription = "ChildActivityPickButton"
		};
		pickButton.Click += async (_, _) =>
		{
			_resultLabel.Text = "Result: Picking...";
			try
			{
				var result = await MediaPicker.PickPhotosAsync();
				_resultLabel.Text = result?.Count > 0
					? $"Result: Got {result.Count} photo(s)"
					: "Result: Cancelled";
			}
			catch (Exception ex)
			{
				_resultLabel.Text = $"Result: Error - {ex.Message}";
			}
		};

		layout.AddView(_resultLabel);
		layout.AddView(pickButton);
		SetContentView(layout);
	}
}
#endif
