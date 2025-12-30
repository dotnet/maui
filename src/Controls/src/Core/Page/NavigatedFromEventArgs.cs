#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public sealed class NavigatedFromEventArgs : EventArgs
	{
		public NavigationType NavigationType { get; }

		public Page DestinationPage { get; }

		internal NavigatedFromEventArgs(Page destinationPage, NavigationType navigationType)
		{
			DestinationPage = destinationPage;
			NavigationType = navigationType;
		}
	}
}