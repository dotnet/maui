#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public sealed class NavigatedToEventArgs : EventArgs
	{
		internal NavigatedToEventArgs(Page previousPage, NavigationType navigationType)
		{
			PreviousPage = previousPage;
			NavigationType = navigationType;
		}

		public Page PreviousPage { get; }
		public NavigationType NavigationType { get; }
	}
}
