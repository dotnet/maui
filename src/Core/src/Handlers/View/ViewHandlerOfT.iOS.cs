using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{
		public new WrapperView? ContainerView
		{
			get => (WrapperView?)base.ContainerView;
			protected set => base.ContainerView = value;
		}

		public UIViewController? ViewController { get; set; }

		public override void PlatformArrange(Rect rect) =>
			this.PlatformArrangeHandler(rect);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{
			if (PlatformView == null || ContainerView != null)
				return;

			var oldParent = (UIView?)PlatformView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(PlatformView);
			PlatformView.RemoveFromSuperview();

			ContainerView ??= new WrapperView(PlatformView.Bounds);
			ContainerView.AddSubview(PlatformView);

			// Re-apply transforms from the cross-platform view model so the wrapper
			// becomes the transform owner when shadows require a container.
			ContainerView.UpdateTransformation(VirtualView);
			PlatformView.ResetLayerTransform();

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(ContainerView, idx);
			else
				oldParent?.AddSubview(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (PlatformView == null || ContainerView == null)
			{
				CleanupContainerView(ContainerView);
				ContainerView = null;
				return;
			}

			if (PlatformView.Superview != ContainerView)
			{
				CleanupContainerView(ContainerView);
				ContainerView = null;

				// Ensure the platform view keeps the current model transform even when
				// the wrapper was no longer the direct parent.
				PlatformView.UpdateTransformation(VirtualView);
				return;
			}

			var oldParent = (UIView?)ContainerView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(ContainerView);

			CleanupContainerView(ContainerView);
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.InsertSubview(PlatformView, idx);
			else
				oldParent?.AddSubview(PlatformView);

			PlatformView.UpdateTransformation(VirtualView);

			void CleanupContainerView(UIView? containerView)
			{
				if (containerView is WrapperView wrapperView)
				{
					wrapperView.RemoveFromSuperview();
					wrapperView.Disconnect();
				}
			}
		}
	}
}