using System;

namespace Microsoft.Maui
{
	public interface IVirtualListViewSelector
	{
		bool SectionHasHeader(int sectionIndex);
		bool SectionHasFooter(int sectionIndex);

		IView CreateView(PositionKind kind, object data, int sectionIndex, int itemIndex = -1);
		void RecycleView(PositionKind kind, object data, IView view, int sectionIndex, int itemIndex = -1);
		string GetReuseId(PositionKind kind, object data, int sectionIndex, int itemIndex);
	}
}
