#nullable disable
using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ListProxyChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ListProxyChangedEventArgs']/Docs/*" />
	public class ListProxyChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ListProxyChangedEventArgs.xml" path="//Member[@MemberName='OldList']/Docs/*" />
		public IReadOnlyCollection<object> OldList { get; }
		/// <include file="../../docs/Microsoft.Maui.Controls/ListProxyChangedEventArgs.xml" path="//Member[@MemberName='NewList']/Docs/*" />
		public IReadOnlyCollection<object> NewList { get; }

		/// <param name="oldList">To be added.</param>
		/// <param name="newList">To be added.</param>
		public ListProxyChangedEventArgs(IReadOnlyCollection<object> oldList, IReadOnlyCollection<object> newList)
		{
			OldList = oldList;
			NewList = newList;
		}
	}
}