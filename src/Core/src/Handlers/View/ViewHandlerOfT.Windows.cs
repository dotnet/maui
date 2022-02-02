#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public abstract partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public override void NativeArrange(Rectangle rect) =>
			this.NativeArrangeHandler(rect);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{
			if (NativeView == null || ContainerView != null)
				return;

			var oldParent = (Panel?)NativeView.Parent;

			var oldIndex = oldParent?.Children.IndexOf(NativeView);
			oldParent?.Children.Remove(NativeView);

			ContainerView ??= new WrapperView();
			((WrapperView)ContainerView).Child = NativeView;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.Children.Insert(idx, ContainerView);
			else
				oldParent?.Children.Add(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (NativeView == null || ContainerView == null || NativeView.Parent != ContainerView)
				return;

			var oldParent = (Panel?)ContainerView.Parent;

			var oldIndex = oldParent?.Children.IndexOf(ContainerView);
			oldParent?.Children.Remove(ContainerView);

			((WrapperView)ContainerView).Child = null;
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.Children.Insert(idx, NativeView);
			else
				oldParent?.Children.Add(NativeView);
		}
	}
}