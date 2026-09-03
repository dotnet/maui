#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests;

internal static class ExternalNavigationTestData
{
	public const string ApplicationId = "com.microsoft.maui.mauiblazorwebview.devicetests";

	public const string ExplicitActivityName = ApplicationId + ".ExplicitIntentTestActivity";

	public const string NonBrowsableActivityName = ApplicationId + ".NonBrowsableIntentTestActivity";
	public const string NonBrowsableAction = ApplicationId + ".action.NON_BROWSABLE";
	public const string NonBrowsableScheme = "maui-blazor-non-browsable";

	public const string BrowsableActivityName = ApplicationId + ".BrowsableIntentTestActivity";
	public const string BrowsableAction = ApplicationId + ".action.BROWSABLE";
	public const string BrowsableScheme = "maui-blazor-browsable";
}

internal static class ActivityLaunchMonitor<TActivity>
	where TActivity : Activity
{
	static TaskCompletionSource<TActivity>? pendingLaunch;

	public static Task<TActivity> PrepareForLaunch()
	{
		var launch = new TaskCompletionSource<TActivity>(TaskCreationOptions.RunContinuationsAsynchronously);
		Interlocked.Exchange(ref pendingLaunch, launch)?.TrySetCanceled();
		return launch.Task;
	}

	public static void RecordLaunch(TActivity activity) =>
		Interlocked.Exchange(ref pendingLaunch, null)?.TrySetResult(activity);

	public static void Reset() =>
		Interlocked.Exchange(ref pendingLaunch, null)?.TrySetCanceled();
}

[Activity(Name = ExternalNavigationTestData.ExplicitActivityName, Exported = false, NoHistory = true)]
public sealed class ExplicitIntentTestActivity : Activity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		ActivityLaunchMonitor<ExplicitIntentTestActivity>.RecordLaunch(this);
		Finish();
	}
}

[Activity(Name = ExternalNavigationTestData.NonBrowsableActivityName, Exported = true, NoHistory = true)]
[IntentFilter(
	new[] { ExternalNavigationTestData.NonBrowsableAction },
	Categories = new[] { Intent.CategoryDefault },
	DataScheme = ExternalNavigationTestData.NonBrowsableScheme)]
public sealed class NonBrowsableIntentTestActivity : Activity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		ActivityLaunchMonitor<NonBrowsableIntentTestActivity>.RecordLaunch(this);
		Finish();
	}
}

[Activity(Name = ExternalNavigationTestData.BrowsableActivityName, Exported = true, NoHistory = true)]
[IntentFilter(
	new[] { ExternalNavigationTestData.BrowsableAction },
	Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
	DataScheme = ExternalNavigationTestData.BrowsableScheme)]
public sealed class BrowsableIntentTestActivity : Activity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		ActivityLaunchMonitor<BrowsableIntentTestActivity>.RecordLaunch(this);
		Finish();
	}
}
