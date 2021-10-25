using System;

namespace Microsoft.Maui
{
	public interface IVirtualListViewSelector
	{
		bool SectionHasHeader(int sectionIndex);
		bool SectionHasFooter(int sectionIndex);

		IView CreateView(PositionInfo position, object data);
		void RecycleView(PositionInfo position, object data, IView view);
		string GetReuseId(PositionInfo position, object data);

		void ViewDetached(PositionInfo position, IView view)
		{ }

		void ViewAttached(PositionInfo position, IView view)
		{ }
	}
}
