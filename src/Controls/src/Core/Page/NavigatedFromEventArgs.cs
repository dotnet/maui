#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	internal enum NavigationType
	{
		Push,
		Pop,
		PopToRoot,
		Insert,
		Remove,
		PageSwap,
		Initialize
	}

	public sealed class NavigatedFromEventArgs : EventArgs
	{
		internal NavigationType NavigationType { get; }

		internal Page DestinationPage { get; }

		internal NavigatedFromEventArgs(Page destinationPage, NavigationType navigationType)
		{
			DestinationPage = destinationPage;
			NavigationType = navigationType;
		}
	}
}
