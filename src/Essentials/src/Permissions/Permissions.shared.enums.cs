namespace Microsoft.Maui.ApplicationModel
{
	/// <summary>
	/// Possible statuses of a permission.
	/// </summary>
	public enum PermissionStatus
	{
		/// <summary>The permission is in an unknown state.</summary>
		Unknown = 0,

		/// <summary>The user denied the permission request</summary>
		Denied = 1,

		/// <summary>The feature is disabled on the device.</summary>
		Disabled = 2,

		/// <summary>The user granted permission or is automatically granted.</summary>
		Granted = 3,

		/// <summary>In a restricted state.</summary>
		Restricted = 4,

		/// <summary>In a limited state (only iOS).</summary>
		Limited = 5
	}
}
