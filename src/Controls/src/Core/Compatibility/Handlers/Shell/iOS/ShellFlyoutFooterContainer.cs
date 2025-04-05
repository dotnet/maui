using CoreGraphics;

namespace Microsoft.Maui.Controls.Platform.Compatibility;

internal class ShellFlyoutFooterContainer : UIContainerView, IPlatformMeasureInvalidationController
{
	bool _invalidateParentWhenMovedToWindow;

	public ShellFlyoutFooterContainer(View view) : base(view) { }

	void IPlatformMeasureInvalidationController.InvalidateAncestorsMeasuresWhenMovedToWindow()
	{
		_invalidateParentWhenMovedToWindow = true;
	}

	void IPlatformMeasureInvalidationController.InvalidateMeasure(bool isPropagating)
	{
		if (Superview is not null)
		{
			var size = SizeThatFits(new CGSize(Superview.Frame.Width, double.PositiveInfinity));
			Frame = new CGRect(0, Superview.Frame.Height - size.Height, Superview.Frame.Width, size.Height);
		}
	}

	public override void MovedToWindow()
	{
		base.MovedToWindow();
		if (_invalidateParentWhenMovedToWindow)
		{
			_invalidateParentWhenMovedToWindow = false;
			this.InvalidateAncestorsMeasures();
		}
	}
}
