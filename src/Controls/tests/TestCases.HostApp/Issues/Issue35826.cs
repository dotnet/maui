using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35826, "PickPhotosAsync hangs when called from a child activity", PlatformAffected.Android)]
public class Issue35826 : ContentPage
{
	static WeakReference<Label> s_statusLabel;

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
		s_statusLabel = new(statusLabel);

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 25,
			Children = { instructions, openButton, statusLabel }
		};
	}

	internal static void UpdateStatus(string text)
	{
		if (s_statusLabel?.TryGetTarget(out var statusLabel) == true)
			statusLabel.Dispatcher.Dispatch(() => statusLabel.Text = text);
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
	Android.Widget.TextView _resultLabel = null!;

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
			Text = "Result: Ready"
		};
		_resultLabel.SetPadding(0, 0, 0, 50);
		SetViewIdResourceName(_resultLabel, "ChildActivityResultLabel");

		var isPhotoPickerAvailable =
			AndroidX.Activity.Result.Contract.ActivityResultContracts.PickVisualMedia.InvokeIsPhotoPickerAvailable(this);
		var pickerAvailabilityLabel = new Android.Widget.TextView(this)
		{
			Text = isPhotoPickerAvailable
				? "Photo Picker: Available"
				: "Photo Picker: Unavailable"
		};
		pickerAvailabilityLabel.SetPadding(0, 0, 0, 50);
		SetViewIdResourceName(pickerAvailabilityLabel, "PhotoPickerAvailabilityLabel");

		var pickButton = new Android.Widget.Button(this)
		{
			Text = "Pick Photos"
		};
		SetViewIdResourceName(pickButton, "ChildActivityPickButton");

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
			catch (OperationCanceledException)
			{
				_resultLabel.Text = "Result: Cancelled";
			}
			catch (Exception ex)
			{
				_resultLabel.Text = $"Result: Error - {ex.Message}";
			}
		};

		var overlapButton = new Android.Widget.Button(this)
		{
			Text = "Start Overlapping Picks"
		};
		SetViewIdResourceName(overlapButton, "ChildActivityOverlapButton");
		overlapButton.Click += async (_, _) =>
		{
			_resultLabel.Text = "Result: Starting overlap...";
			var firstRequest = MediaPicker.PickPhotosAsync();
			try
			{
				await MediaPicker.PickPhotosAsync();
				_resultLabel.Text = "Result: Overlap Not Rejected";
			}
			catch (InvalidOperationException)
			{
				_resultLabel.Text = "Result: Overlap Rejected";
			}
			catch (Exception ex)
			{
				_resultLabel.Text = $"Result: Error - {ex.Message}";
			}

			try
			{
				await firstRequest;
			}
			catch (OperationCanceledException)
			{
			}
		};

		var finishWhilePickingButton = new Android.Widget.Button(this)
		{
			Text = "Pick Photos And Finish Activity"
		};
		SetViewIdResourceName(finishWhilePickingButton, "ChildActivityFinishWhilePickingButton");
		finishWhilePickingButton.Click += async (_, _) =>
		{
			Issue35826.UpdateStatus("Status: Waiting for launch-activity cancellation");
			var request = MediaPicker.PickPhotosAsync();
			Finish();

			try
			{
				await request;
				Issue35826.UpdateStatus("Status: Launch activity completed unexpectedly");
			}
			catch (OperationCanceledException)
			{
				Issue35826.UpdateStatus("Status: Launch activity cancelled");
			}
			catch (Exception ex)
			{
				Issue35826.UpdateStatus($"Status: Error - {ex.Message}");
			}
		};

		layout.AddView(pickerAvailabilityLabel);
		layout.AddView(_resultLabel);
		layout.AddView(pickButton);
		layout.AddView(overlapButton);
		layout.AddView(finishWhilePickingButton);
		SetContentView(layout);
	}

	// Sets ViewIdResourceName on a native Android view so Appium can locate it by
	// resource-id (the same mechanism MAUI uses for AutomationId on Android).
	void SetViewIdResourceName(Android.Views.View view, string automationId)
	{
		var resourceName = $"{PackageName}:id/{automationId}";
		AndroidX.Core.View.ViewCompat.SetAccessibilityDelegate(view, new AutomationIdDelegate(resourceName));
	}

	class AutomationIdDelegate : AndroidX.Core.View.AccessibilityDelegateCompat
	{
		readonly string _resourceName;

		public AutomationIdDelegate(string resourceName) => _resourceName = resourceName;

		public override void OnInitializeAccessibilityNodeInfo(Android.Views.View host, AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat info)
		{
			base.OnInitializeAccessibilityNodeInfo(host, info);
			info.ViewIdResourceName = _resourceName;
		}
	}
}
#endif
