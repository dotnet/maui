#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemTappedEventArgs']/Docs/*" />
	public class ItemTappedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ItemTappedEventArgs(object group, object item, int itemIndex)
		{
			Group = group;
			Item = item;
			ItemIndex = itemIndex;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="//Member[@MemberName='Group']/Docs/*" />
		public object Group { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="//Member[@MemberName='Item']/Docs/*" />
		public object Item { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="//Member[@MemberName='ItemIndex']/Docs/*" />
		public int ItemIndex { get; private set; }
	}
}