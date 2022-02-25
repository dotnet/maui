using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{
	public delegate void FoldingFeatureChangedHandler(object sender, System.EventArgs ea);

	public interface IFoldableContext
	{
		/// <summary>Whether an obscuring hinge exists or a foldable is in Flex Mode (HALF_OPEN)</summary>
		bool isSeparating { get; set; }
		/// <summary>Coordinates of the obscured hinge area (within the WindowBounds, in pixels)</summary>
		Rectangle FoldingFeatureBounds { get; set;  }
		/// <summary>Size of the screen (in pixels)</summary>
		Rectangle WindowBounds { get; set; }
		/// <summary>Density is required to convert px to dp for layout measurements (eg. 2.5 for Surface Duo)</summary>
		float ScreenDensity { get; set; }
		/// <summary>Event triggered when the app is spanned or unspanned or rotated while spanned</summary>
		event System.EventHandler<FoldEventArgs> FoldingFeatureChanged;
	}
}
