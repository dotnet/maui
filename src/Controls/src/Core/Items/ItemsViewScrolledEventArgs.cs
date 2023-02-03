#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemsViewScrolledEventArgs']/Docs/*" />
	public class ItemsViewScrolledEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='HorizontalDelta']/Docs/*" />
		public double HorizontalDelta { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='VerticalDelta']/Docs/*" />
		public double VerticalDelta { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='HorizontalOffset']/Docs/*" />
		public double HorizontalOffset { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='VerticalOffset']/Docs/*" />
		public double VerticalOffset { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='FirstVisibleItemIndex']/Docs/*" />
		public int FirstVisibleItemIndex { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='CenterItemIndex']/Docs/*" />
		public int CenterItemIndex { get; set; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsViewScrolledEventArgs.xml" path="//Member[@MemberName='LastVisibleItemIndex']/Docs/*" />
		public int LastVisibleItemIndex { get; set; }
	}
}