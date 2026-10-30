using System;

namespace Microsoft.Maui.Controls
{
	// These are copied from UWP so if you add additional values please use those
	// https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.datatransfer.datapackageoperation?view=winrt-19041
	/// <summary>
	/// Specifies the type of operation performed during a drag and drop operation.
	/// </summary>
	[Flags]
	public enum DataPackageOperation
	{
		/// <summary>
		/// No operation is performed.
		/// </summary>
		None = 0,

		/// <summary>
		/// The data is copied to the drop target.
		/// </summary>
		Copy = 1,

		/// <summary>
		/// The data is moved to the drop target.
		/// </summary>
		/// <remarks>
		/// The drag source must allow moving. The application is responsible for updating
		/// its data or collections when the drop succeeds; this operation does not remove
		/// the source data or move a view automatically.
		/// </remarks>
		Move = 2
	}
}
