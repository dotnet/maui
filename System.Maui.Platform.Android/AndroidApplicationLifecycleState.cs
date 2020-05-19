namespace System.Maui.Platform.Android
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