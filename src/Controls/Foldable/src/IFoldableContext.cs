using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Foldable
{
	delegate void FoldingFeatureChangedHandler(object sender, System.EventArgs ea);

	interface IFoldableContext
	{
		/// <summary>Whether an obscuring hinge exists or a foldable is in Flex Mode (HALF_OPEN)</summary>
		bool IsSeparating { get; set; }
		/// <summary>Coordinates of the obscured hinge area (within the WindowBounds, in pixels)</summary>
		Rect FoldingFeatureBounds { get; set;  }
		/// <summary>Size of the screen (in pixels)</summary>
		Rect WindowBounds { get; set; }
	}
}
