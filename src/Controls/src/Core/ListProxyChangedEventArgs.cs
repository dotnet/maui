using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	public class ListProxyChangedEventArgs : EventArgs
	{
		public IReadOnlyCollection<object> OldList { get; }
		public IReadOnlyCollection<object> NewList { get; }

		public ListProxyChangedEventArgs(IReadOnlyCollection<object> oldList, IReadOnlyCollection<object> newList)
		{
			OldList = oldList;
			NewList = newList;
		}
	}
}