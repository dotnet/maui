using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{
	public interface IFoldableContext : AndroidX.Core.Util.IConsumer
	{
		bool isSeparating { get; }
		Rectangle FoldingFeatureBounds { get; }
		Rectangle WindowBounds { get; }
		
	}
}
