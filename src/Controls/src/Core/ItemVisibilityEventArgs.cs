using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ItemVisibilityEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemVisibilityEventArgs']/Docs" />
	public sealed class ItemVisibilityEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ItemVisibilityEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ItemVisibilityEventArgs(object item, int itemIndex)
		{
			Item = item;
			ItemIndex = itemIndex;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemVisibilityEventArgs.xml" path="//Member[@MemberName='Item']/Docs" />
		public object Item { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ItemVisibilityEventArgs.xml" path="//Member[@MemberName='ItemIndex']/Docs" />
		public int ItemIndex { get; private set; }
	}
}