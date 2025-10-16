using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsUpdatingScrollMode.xml" path="Type[@FullName='Microsoft.Maui.Controls.ItemsUpdatingScrollMode']/Docs/*" />
	public enum ItemsUpdatingScrollMode
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsUpdatingScrollMode.xml" path="//Member[@MemberName='KeepItemsInView']/Docs/*" />
		/// <summary>KeepItemsInView keeps the first item in the list displayed when new items are added.</summary>
		KeepItemsInView = 0,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsUpdatingScrollMode.xml" path="//Member[@MemberName='KeepScrollOffset']/Docs/*" />
		/// <summary>KeepScrollOffset ensures that the current scroll position is maintained when new items are added.</summary>
		KeepScrollOffset,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ItemsUpdatingScrollMode.xml" path="//Member[@MemberName='KeepLastItemInView']/Docs/*" />
		/// <summary>KeepLastItemInView adjusts the scroll offset to keep the last item in the list displayed when new items are added.</summary>
		KeepLastItemInView
	}
}
