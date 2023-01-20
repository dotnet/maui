#nullable disable
using System;
namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/CurrentItemChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.CurrentItemChangedEventArgs']/Docs/*" />
	public class CurrentItemChangedEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/CurrentItemChangedEventArgs.xml" path="//Member[@MemberName='PreviousItem']/Docs/*" />
		public object PreviousItem { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/CurrentItemChangedEventArgs.xml" path="//Member[@MemberName='CurrentItem']/Docs/*" />
		public object CurrentItem { get; }

		internal CurrentItemChangedEventArgs(object previousItem, object currentItem)
		{
			PreviousItem = previousItem;
			CurrentItem = currentItem;
		}
	}
}
