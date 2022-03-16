#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{
		public override void PlatformArrange(Rect rect) =>
			this.PlatformArrangeHandler(rect);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{
			if (PlatformView == null || ContainerView != null)
				return;

			var oldParent = (Panel?)PlatformView.Parent;

			var oldIndex = oldParent?.Children.IndexOf(PlatformView);
			oldParent?.Children.Remove(PlatformView);

			ContainerView ??= new WrapperView();
			((WrapperView)ContainerView).Child = PlatformView;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.Children.Insert(idx, ContainerView);
			else
				oldParent?.Children.Add(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (PlatformView == null || ContainerView == null || PlatformView.Parent != ContainerView)
				return;

			var oldParent = (Panel?)ContainerView.Parent;

			var oldIndex = oldParent?.Children.IndexOf(ContainerView);
			oldParent?.Children.Remove(ContainerView);

			((WrapperView)ContainerView).Child = null;
			((WrapperView)ContainerView).Dispose();
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.Children.Insert(idx, PlatformView);
			else
				oldParent?.Children.Add(PlatformView);
		}
	}
}