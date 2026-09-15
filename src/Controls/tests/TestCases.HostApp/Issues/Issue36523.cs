using Microsoft.Maui.Media;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36523, "MediaPicker.PickPhotosAsync hangs after device rotation on API 33+", PlatformAffected.Android)]
public class Issue36523 : ContentPage
{
	public Issue36523()
	{
		var openButton = new Button { AutomationId = "OpenRotationActivityButton", Text = "Open Rotation Activity" };
		openButton.Clicked += (_, _) =>
		{
#if ANDROID
			Issue36523State.Reset();
			var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity!;
			activity.StartActivity(new Android.Content.Intent(activity, typeof(Issue36523RotationActivity)));
#endif
		};
		Content = new VerticalStackLayout { Padding = 30, Children = { openButton } };
	}
}

#if ANDROID
static class Issue36523State
{
	static readonly object s_gate = new();
	static Task s_pickerTask;
	static int s_launchActivityId;
	static string s_outcome = "READY";

	public static void Reset() { lock (s_gate) { s_pickerTask = null; s_launchActivityId = 0; s_outcome = "READY"; } }
	public static string GetOutcome() { lock (s_gate) return s_outcome; }
	public static void Complete(string outcome) { lock (s_gate) s_outcome = outcome; }

	public static void BeginPick(int activityId, Task task)
	{
		lock (s_gate)
		{ s_launchActivityId = activityId; s_pickerTask = task; s_outcome = "WAITING"; }
	}

	public static bool CheckForHang(int currentActivityId)
	{
		lock (s_gate)
		{
			if (s_pickerTask is { IsCompleted: false } && s_launchActivityId != currentActivityId)
			{
				s_outcome = "FAIL: picker task hung after activity recreation";
				return true;
			}
			return false;
		}
	}
}

[Android.App.Activity(Label = "Issue36523", Theme = "@style/Maui.SplashTheme")]
public class Issue36523RotationActivity : AndroidX.AppCompat.App.AppCompatActivity
{
	static int s_nextId;
	readonly int _id = Interlocked.Increment(ref s_nextId);
	Android.Widget.TextView _status = null!;
	CancellationTokenSource _cts;

	protected override void OnCreate(Android.OS.Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		Microsoft.Maui.ApplicationModel.Platform.Init(this, savedInstanceState);

		var layout = new Android.Widget.LinearLayout(this) { Orientation = Android.Widget.Orientation.Vertical };
		var statusBarHeight = 0;
		var resourceId = Resources.GetIdentifier("status_bar_height", "dimen", "android");
		if (resourceId > 0)
		{
			statusBarHeight = Resources.GetDimensionPixelSize(resourceId);
		}
		layout.SetPadding(50, statusBarHeight + 50, 50, 50);

		_status = new Android.Widget.TextView(this) { Text = $"Status: {Issue36523State.GetOutcome()}" };
		SetAutomationId(_status, "RotationActivityStatusLabel");

		var btn = new Android.Widget.Button(this) { Text = "Pick Photos" };
		SetAutomationId(btn, "RotationActivityPickButton");
		btn.Click += OnPick;

		layout.AddView(_status);
		layout.AddView(btn);
		SetContentView(layout);
	}

	public override void OnWindowFocusChanged(bool hasFocus)
	{
		base.OnWindowFocusChanged(hasFocus);
		_cts?.Cancel();
		_cts?.Dispose();
		_cts = null;
		if (!hasFocus)
		{
			return;
		}
		_cts = new CancellationTokenSource();
		_ = DelayedHangCheck(_cts.Token);
	}

	async Task DelayedHangCheck(CancellationToken ct)
	{
		try
		{
			await Task.Delay(3000, ct);
			if (!IsDestroyed)
			{
				RunOnUiThread(() => { Issue36523State.CheckForHang(_id); _status.Text = $"Status: {Issue36523State.GetOutcome()}"; });
			}
		}
		catch (OperationCanceledException) { }
	}

	async void OnPick(object s, EventArgs e)
	{
		_status.Text = "Status: WAITING";
		try
		{
			var task = MediaPicker.PickPhotosAsync();
			Issue36523State.BeginPick(_id, task);
			var r = await task;
			Issue36523State.Complete(r?.Count > 0 ? $"PASS: got {r.Count} photo(s)" : "PASS: cancelled");
		}
		catch (OperationCanceledException) { Issue36523State.Complete("PASS: cancelled"); }
		catch (Exception ex) { Issue36523State.Complete($"ERROR: {ex.Message}"); }
		if (!IsDestroyed)
		{
			_status.Text = $"Status: {Issue36523State.GetOutcome()}";
		}
	}

	protected override void OnDestroy() { _cts?.Cancel(); _cts?.Dispose(); _cts = null; base.OnDestroy(); }

	void SetAutomationId(Android.Views.View view, string id)
	{
		AndroidX.Core.View.ViewCompat.SetAccessibilityDelegate(view,
			new IdDelegate($"{PackageName}:id/{id}"));
	}

	class IdDelegate(string name) : AndroidX.Core.View.AccessibilityDelegateCompat
	{
		public override void OnInitializeAccessibilityNodeInfo(Android.Views.View host,
			AndroidX.Core.View.Accessibility.AccessibilityNodeInfoCompat info)
		{ base.OnInitializeAccessibilityNodeInfo(host, info); info.ViewIdResourceName = name; }
	}
}
#endif
