namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/PermissionStatus.xml" path="Type[@FullName='Microsoft.Maui.Essentials.PermissionStatus']/Docs" />
	public enum PermissionStatus
	{
		// Permission is in an unknown state
		/// <include file="../../docs/Microsoft.Maui.Essentials/PermissionStatus.xml" path="//Member[@MemberName='Unknown']/Docs" />
		Unknown = 0,

		// Denied by user
		/// <include file="../../docs/Microsoft.Maui.Essentials/PermissionStatus.xml" path="//Member[@MemberName='Denied']/Docs" />
		Denied = 1,

		// Feature is disabled on device
		/// <include file="../../docs/Microsoft.Maui.Essentials/PermissionStatus.xml" path="//Member[@MemberName='Disabled']/Docs" />
		Disabled = 2,

		// Granted by user
		/// <include file="../../docs/Microsoft.Maui.Essentials/PermissionStatus.xml" path="//Member[@MemberName='Granted']/Docs" />
		Granted = 3,

		// Restricted (only iOS)
		/// <include file="../../docs/Microsoft.Maui.Essentials/PermissionStatus.xml" path="//Member[@MemberName='Restricted']/Docs" />
		Restricted = 4
	}
}
