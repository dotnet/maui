using CoreGraphics;

namespace Microsoft.Maui.Controls.Platform.Compatibility;

internal class ShellFlyoutFooterContainer : UIContainerView, IPlatformMeasureInvalidationController
{
	public ShellFlyoutFooterContainer(View view) : base(view) { }

	void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow() { }

	void IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
	{
		var size = SizeThatFits(new CGSize(View.Frame.Width, double.PositiveInfinity));
		Frame = new CGRect(0, Superview.Frame.Height - size.Height, Superview.Frame.Width, size.Height);
	}
}
