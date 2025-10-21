#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="Microsoft.Maui.Controls.ListView.ItemTapped"/> event.</summary>
	public class ItemTappedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ItemTappedEventArgs(object group, object item, int itemIndex)
		{
			Group = group;
			Item = item;
			ItemIndex = itemIndex;
		}

		/// <summary>The collection of elements to which the tapped item belongs.</summary>
		public object Group { get; private set; }

		/// <summary>The visual element that the user tapped.</summary>
		public object Item { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemTappedEventArgs.xml" path="//Member[@MemberName='ItemIndex']/Docs/*" />
		public int ItemIndex { get; private set; }
	}
}