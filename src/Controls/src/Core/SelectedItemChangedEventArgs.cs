using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SelectedItemChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SelectedItemChangedEventArgs']/Docs" />
	public class SelectedItemChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SelectedItemChangedEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public SelectedItemChangedEventArgs(object selectedItem, int selectedItemIndex)
		{
			SelectedItem = selectedItem;
			SelectedItemIndex = selectedItemIndex;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SelectedItemChangedEventArgs.xml" path="//Member[@MemberName='SelectedItem']/Docs" />
		public object SelectedItem { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/SelectedItemChangedEventArgs.xml" path="//Member[@MemberName='SelectedItemIndex']/Docs" />
		public int SelectedItemIndex { get; private set; }

	}
}