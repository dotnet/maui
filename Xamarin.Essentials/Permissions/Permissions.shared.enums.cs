namespace Xamarin.Essentials
{
    enum PermissionStatus
    {
        // Denied by user
        Denied,

        // Feature is disabled on device
        Disabled,

        // Granted by user
        Granted,

        // Restricted (only iOS)
        Restricted,

        // Permission is in an unknown state
        Unknown
    }

    enum PermissionType
    {
        Unknown,
        Battery,
        Camera,
        Flashlight,
        LaunchApp,
        LocationWhenInUse,
        Maps,
        NetworkState,
        Vibrate,
        WriteExternalStorage,
    }
}
