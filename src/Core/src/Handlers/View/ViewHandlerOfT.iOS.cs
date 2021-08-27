using Microsoft.Maui.Graphics;
using UIKit;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		UIView? INativeViewHandler.NativeView => WrappedNativeView;
		UIView? INativeViewHandler.ContainerView => ContainerView;

		protected new UIView? WrappedNativeView =>
			(UIView?)base.WrappedNativeView;

		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		UIViewController? INativeViewHandler.ViewController => null;

		public override void NativeArrange(Rectangle rect)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null)
				return;

			// We set Center and Bounds rather than Frame because Frame is undefined if the CALayer's transform is 
			// anything other than the identity (https://developer.apple.com/documentation/uikit/uiview/1622459-transform)
			nativeView.Center = new CoreGraphics.CGPoint(rect.Center.X, rect.Center.Y);

			// The position of Bounds is usually (0,0), but in some cases (e.g., UIScrollView) it's the content offset.
			// So just leave it a whatever value iOS thinks it should be.
			nativeView.Bounds = new CoreGraphics.CGRect(nativeView.Bounds.X, nativeView.Bounds.Y, rect.Width, rect.Height);

			nativeView.UpdateBackgroundLayerFrame();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var nativeView = WrappedNativeView;

			if (nativeView == null)
			{
				return new Size(widthConstraint, heightConstraint);
			}

			var sizeThatFits = nativeView.SizeThatFits(new CoreGraphics.CGSize((float)widthConstraint, (float)heightConstraint));

			var size = new Size(
				sizeThatFits.Width == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Width,
				sizeThatFits.Height == float.PositiveInfinity ? double.PositiveInfinity : sizeThatFits.Height);

			if (double.IsInfinity(size.Width) || double.IsInfinity(size.Height))
			{
				nativeView.SizeToFit();
				size = new Size(nativeView.Frame.Width, nativeView.Frame.Height);
			}

			var finalWidth = ResolveConstraints(size.Width, VirtualView.Width, VirtualView.MinimumWidth, VirtualView.MaximumWidth);
			var finalHeight = ResolveConstraints(size.Height, VirtualView.Height, VirtualView.MinimumHeight, VirtualView.MaximumHeight);

			return new Size(finalWidth, finalHeight);
		}

		double ResolveConstraints(double measured, double exact, double min, double max) 
		{
			var resolved = measured;

			if (IsExplicitSet(exact))
			{
				// If an exact value has been specified, try to use that
				resolved = exact;
			}

			if (resolved > max)
			{
				// Apply the max value constraint (if any)
				// If the exact value is in conflict with the max value, the max value should win
				resolved = max;
			}

			if (resolved < min)
			{
				// Apply the min value constraint (if any)
				// If the exact or max value is in conflict with the min value, the min value should win
				resolved = min;
			}

			return resolved;
		}

		protected override void SetupContainer()
		{
			if (NativeView == null || ContainerView != null)
				return;

			var oldParent = (UIView?)NativeView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(NativeView);
			NativeView.RemoveFromSuperview();

			ContainerView ??= new WrapperView(NativeView.Bounds);
			ContainerView.AddSubview(NativeView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(ContainerView, idx);
			else
				oldParent?.AddSubview(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (NativeView == null || ContainerView == null || NativeView.Superview != ContainerView)
				return;

			var oldParent = (UIView?)ContainerView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(ContainerView);
			ContainerView.RemoveFromSuperview();

			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(NativeView, idx);
			else
				oldParent?.AddSubview(NativeView);
		}
	}
}