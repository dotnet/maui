#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public sealed class NavigatedToEventArgs : EventArgs
	{
		internal NavigatedToEventArgs(Page previousPage)
		{
			PreviousPage = previousPage;
		}

		internal Page PreviousPage { get; }
	}
}
