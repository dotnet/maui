#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ScrollToRequestEventArgs']/Docs/*" />
	public class ScrollToRequestEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='Mode']/Docs/*" />
		public ScrollToMode Mode { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='ScrollToPosition']/Docs/*" />
		public ScrollToPosition ScrollToPosition { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='IsAnimated']/Docs/*" />
		public bool IsAnimated { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='Index']/Docs/*" />
		public int Index { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='GroupIndex']/Docs/*" />
		public int GroupIndex { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='Item']/Docs/*" />
		public object Item { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='Group']/Docs/*" />
		public object Group { get; }

		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public ScrollToRequestEventArgs(int index, int groupIndex,
			ScrollToPosition scrollToPosition, bool isAnimated)
		{
			Mode = ScrollToMode.Position;

			Index = index;
			GroupIndex = groupIndex;
			ScrollToPosition = scrollToPosition;
			IsAnimated = isAnimated;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/ScrollToRequestEventArgs.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ScrollToRequestEventArgs(object item, object group,
			ScrollToPosition scrollToPosition, bool isAnimated)
		{
			Mode = ScrollToMode.Element;

			Item = item;
			Group = group;
			ScrollToPosition = scrollToPosition;
			IsAnimated = isAnimated;
		}
	}
}
