namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal enum AndroidApplicationLifecycleState
	{
		Uninitialized,
		OnCreate,
		OnStart,
		OnResume,
		OnPause,
		OnStop,
		OnRestart,
		OnDestroy
	}
}