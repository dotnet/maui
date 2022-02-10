using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public UIViewController? ViewController { get; set; }

		public override void NativeArrange(Rectangle rect) =>
			this.NativeArrangeHandler(rect);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

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