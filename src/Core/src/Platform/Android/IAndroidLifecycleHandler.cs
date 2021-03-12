using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;

namespace Microsoft.Maui
{
	/// <summary>
	/// Allow to get Android Activity lifecycle callbacks.
	/// </summary>
	public interface IAndroidLifecycleHandler : IPlatformLifecycleHandler
	{
		/// <summary>
		/// Called when the activity is starting.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		/// <param name="savedInstanceState">Previous state saved</param>
		void OnCreate(Activity activity, Bundle? savedInstanceState);

		/// <summary>
		/// Called when the activity had been stopped, but is now again being displayed to the user. 
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks</param>
		void OnStart(Activity activity);

		/// <summary>
		/// Called for your activity to start interacting with the user.
		/// This is an indicator that the activity became active and ready to receive input.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks</param>
		void OnResume(Activity activity);

		/// <summary>
		/// Called as part of the activity lifecycle when the user no longer actively interacts with the activity,
		/// but it is still visible on screen.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		void OnPause(Activity activity);

		/// <summary>
		/// Called when you are no longer visible to the user.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks</param>
		void OnStop(Activity activity);

		/// <summary>
		/// Called after OnStop when the current activity is being re-displayed to the user (the user has navigated back to it).
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks</param>
		void OnRestart(Activity activity);

		/// <summary>
		/// This can happen either because the activity is finishing, or because the system is
		/// temporarily destroying this instance of the activity to save space. 
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		void OnDestroy(Activity activity);

		/// <summary>
		/// Called to retrieve per-instance state from an activity before being killed so that the state can be
		/// restored in OnCreate or OnRestoreInstanceState.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		/// <param name="outState">Bundle in which to place your saved state.</param>
		void OnSaveInstanceState(Activity activity, Bundle outState);

		/// <summary>
		/// Restore the state of the dialog from a previously saved bundle.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		/// <param name="savedInstanceState">The state of the dialog previously saved.</param>
		void OnRestoreInstanceState(Activity activity, Bundle savedInstanceState);

		/// <summary>
		/// Called when the current configuration of the resources being used by the application have changed.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		/// <param name="newConfig">The new resource configuration.</param>
		void OnConfigurationChanged(Activity activity, Configuration newConfig);

		/// <summary>
		/// Called when an activity you launched exits, giving you the requestCode you started it with,
		/// the resultCode it returned, and any additional data from it.
		/// </summary>
		/// <param name="activity">The activity on which we receive lifecycle events callbacks.</param>
		/// <param name="requestCode">The integer request code originally supplied to startActivityForResult(), allowing you to identify who this result came from.</param>
		/// <param name="resultCode">The integer result code returned by the child activity.</param>
		/// <param name="data">An Intent, which can return result data to the caller.</param>
		void OnActivityResult(Activity activity, int requestCode, [GeneratedEnum] Result resultCode, Intent? data);
	}
}