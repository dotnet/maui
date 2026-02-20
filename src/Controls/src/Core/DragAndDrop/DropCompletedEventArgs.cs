using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Provides data for the event raised when a drop operation completes.
	/// </summary>
	public class DropCompletedEventArgs : EventArgs
	{
		DataPackageOperation DropResult { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DropCompletedEventArgs"/> class.
		/// </summary>
		public DropCompletedEventArgs()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DropCompletedEventArgs"/> class.
		/// </summary>
		/// <param name="platformArgs">The platform-specific data associated with the drag.</param>
		internal DropCompletedEventArgs(PlatformDropCompletedEventArgs platformArgs) : this()
		{
			PlatformArgs = platformArgs;
		}

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="DropCompletedEventArgs"/>.
		/// </summary>
		public PlatformDropCompletedEventArgs? PlatformArgs { get; }
	}
}
