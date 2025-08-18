#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event args when an item's visibility has been changed in a <see cref="Microsoft.Maui.Controls.ListView"/>.</summary>
	public sealed class ItemVisibilityEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ItemVisibilityEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ItemVisibilityEventArgs(object item, int itemIndex)
		{
			Item = item;
			ItemIndex = itemIndex;
		}

		/// <summary>The item from the <see cref="Microsoft.Maui.Controls.ItemsView{T}.ItemsSource"/> whose visibility has changed.</summary>
		public object Item { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemVisibilityEventArgs.xml" path="//Member[@MemberName='ItemIndex']/Docs/*" />
		public int ItemIndex { get; private set; }
	}
}