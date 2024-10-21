using Android.Content;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Lifecycle;
using JavaObject = Java.Lang.Object;

namespace Maui.Controls.Sample.Platform;

public class Issue14037LifecycleObserver(ActivityResultRegistry registry) : JavaObject, IDefaultLifecycleObserver
{
	readonly ActivityResultRegistry _registry = registry;

	static ActivityResultLauncher Launcher;
	static TaskCompletionSource<ActivityResult> Tcs = null;

	public void OnCreate(ILifecycleOwner owner) =>
		Launcher = _registry.Register(
			nameof(ActivityResultContracts.StartActivityForResult),
			owner,
			new ActivityResultContracts.StartActivityForResult(),
			new ActivityResultCallback<ActivityResult>(activityResult => Tcs?.SetResult(activityResult)));

	public static Task<ActivityResult> Launch()
	{
		Tcs = new TaskCompletionSource<ActivityResult>();

		try
		{
			var intent = new Intent(Microsoft.Maui.ApplicationModel.Platform.CurrentActivity, typeof(Issue14037Activity));
			Launcher?.Launch(intent);
		}
		catch (Exception ex)
		{
			Tcs.SetException(ex);
		}

		return Tcs.Task;
	}

	public void OnStart(ILifecycleOwner owner) { }
	public void OnDestroy(ILifecycleOwner owner) { }
	public void OnPause(ILifecycleOwner owner) { }
	public void OnResume(ILifecycleOwner owner) { }
	public void OnStop(ILifecycleOwner owner) { }
}

public class ActivityResultCallback<T>(Action<T> callback) : JavaObject, IActivityResultCallback
	where T : JavaObject
{
	readonly Action<T> _callback = callback;

	public void OnActivityResult(JavaObject result)
	{
		if (result is T obj)
		{
			_callback(obj);
		}
		else
		{
			_callback(null);
		}
	}
}