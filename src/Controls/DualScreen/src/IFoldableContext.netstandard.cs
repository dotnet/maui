using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{
	public interface IFoldableContext
	{
		bool isSeparating { get; }
		Rectangle FoldingFeatureBounds { get; }

		Rectangle WindowBounds { get; }
	}
}
