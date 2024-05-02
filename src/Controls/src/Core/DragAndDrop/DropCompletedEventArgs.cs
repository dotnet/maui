using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DropCompletedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DropCompletedEventArgs']/Docs/*" />
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
