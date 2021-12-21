using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{
	public delegate void FoldingFeatureChangedHandler(object sender, System.EventArgs ea);

	public interface IFoldableContext //: AndroidX.Core.Util.IConsumer
	{
		bool isSeparating { get; set; }
		Rectangle FoldingFeatureBounds { get; set;  }
		Rectangle WindowBounds { get; set; }
		event System.EventHandler<FoldEventArgs> FoldingFeatureChanged;
	}
}
