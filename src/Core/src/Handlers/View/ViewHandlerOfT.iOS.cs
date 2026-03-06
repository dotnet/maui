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

			InsertViewIntoParent(ContainerView, oldParent, oldIndex);
		}

		protected override void RemoveContainer()
		{
			if (PlatformView == null || ContainerView == null || PlatformView.Superview != ContainerView)
			{
				CleanupContainerView(ContainerView);
				ContainerView = null;
				return;
			}

			var oldParent = (UIView?)ContainerView.Superview;

			var oldIndex = oldParent?.IndexOfSubview(ContainerView);
			CleanupContainerView(ContainerView);
			ContainerView = null;

			InsertViewIntoParent(PlatformView, oldParent, oldIndex);

			void CleanupContainerView(UIView? containerView)
			{
				if (containerView is WrapperView wrapperView)
				{
					wrapperView.RemoveFromSuperview();
					wrapperView.Disconnect();
				}
			}
		}

		private static void InsertViewIntoParent(UIView view, UIView? oldParent, int? oldIndex)
		{
			if (oldParent is ContentView contentView && contentView.View is IBorderView)
			{
				// Border controls need special handling to ensure proper z-ordering
				// of content above background layers when clipping is applied
				contentView.AddSubview(view);
			}
			else if (oldIndex is int idx && idx >= 0)
			{
				oldParent?.InsertSubview(view, idx);
			}
			else
			{
				oldParent?.AddSubview(view);
			}
		}
	}
}