#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public enum NavigationType
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
		public NavigationType NavigationType { get; }

		public Page DestinationPage { get; }

		internal NavigatedFromEventArgs(Page destinationPage, NavigationType navigationType)
		{
			DestinationPage = destinationPage;
			NavigationType = navigationType;
		}
	}
}
