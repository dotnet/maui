#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public sealed class NavigatedFromEventArgs : EventArgs
	{
		internal NavigatedFromEventArgs(Page destinationPage)
		{
			DestinationPage = destinationPage;
		}

		internal Page DestinationPage { get; }
	}
}
