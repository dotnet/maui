using System;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : INativeViewHandler
	{
		public Context Context => MauiContext?.Context ?? throw new InvalidOperationException($"Context cannot be null here");

		public override void NativeArrange(Rectangle frame) =>
			this.NativeArrangeHandler(frame);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{
			if (Context == null || NativeView == null || ContainerView != null)
				return;

			var oldParent = (ViewGroup?)NativeView.Parent;

			var oldIndex = oldParent?.IndexOfChild(NativeView);
			oldParent?.RemoveView(NativeView);

			ContainerView ??= new WrapperView(Context);
			((ViewGroup)ContainerView).AddView(NativeView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(ContainerView, idx);
			else
				oldParent?.AddView(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (Context == null || NativeView == null || ContainerView == null || NativeView.Parent != ContainerView)
				return;

			var oldParent = (ViewGroup?)ContainerView.Parent;

			var oldIndex = oldParent?.IndexOfChild(ContainerView);
			oldParent?.RemoveView(ContainerView);

			((ViewGroup)ContainerView).RemoveAllViews();
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(NativeView, idx);
			else
				oldParent?.AddView(NativeView);
		}
	}
}
