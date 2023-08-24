using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DropCompletedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DropCompletedEventArgs']/Docs/*" />
	public class DropCompletedEventArgs : EventArgs
	{
		DataPackageOperation DropResult { get; }

		/// <summary>
		/// Gets the platform-specific arguments associated with the <see cref="DropCompletedEventArgs"/>.
		/// </summary>
		public PlatformDropCompletedEventArgs? PlatformArgs { get; internal set; }
	}
}
