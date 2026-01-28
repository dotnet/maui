#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for when the internal list proxy changes.</summary>
	public class ListProxyChangedEventArgs : EventArgs
	{
		/// <summary>Gets the previous list proxy.</summary>
		public IReadOnlyCollection<object> OldList { get; }

		/// <summary>Gets the new list proxy.</summary>
		public IReadOnlyCollection<object> NewList { get; }

		/// <summary>Creates a new <see cref="ListProxyChangedEventArgs"/> with the specified lists.</summary>
		/// <param name="oldList">The previous list proxy.</param>
		/// <param name="newList">The new list proxy.</param>
		public ListProxyChangedEventArgs(IReadOnlyCollection<object> oldList, IReadOnlyCollection<object> newList)
		{
			OldList = oldList;
			NewList = newList;
		}
	}
}