namespace Microsoft.Maui.Essentials
{
	public enum PermissionStatus
	{
		// Permission is in an unknown state
		Unknown = 0,

		// Denied by user
		Denied = 1,

		// Feature is disabled on device
		Disabled = 2,

		// Granted by user
		Granted = 3,

		// Restricted (only iOS)
		Restricted = 4
	}
}
