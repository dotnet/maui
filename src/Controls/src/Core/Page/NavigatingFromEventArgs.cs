#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public sealed class NavigatingFromEventArgs : EventArgs
	{
		internal NavigatingFromEventArgs(Page destinationPage, NavigationType navigationType)
		{
			DestinationPage = destinationPage;
			NavigationType = navigationType;
		}

		public NavigationType NavigationType { get; }

		public Page DestinationPage { get; }
	}
}
