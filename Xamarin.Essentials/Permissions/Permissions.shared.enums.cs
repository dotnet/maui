namespace Xamarin.Essentials
{
    internal enum PermissionStatus
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

    internal enum PermissionType
    {
        Unknown,
        Battery,
        Camera,
        Flashlight,
        LocationWhenInUse,
        NetworkState,
        Vibrate,
    }
}
