using System;
using System.Runtime.CompilerServices;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Primitives.Dimension;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TPlatformView> : IPlatformViewHandler
	{
		public Context Context => MauiContext?.Context ?? throw new InvalidOperationException($"Context cannot be null here");

		public override void PlatformArrange(Rect frame) =>
			this.PlatformArrangeHandler(frame);

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint) =>
			this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);

		protected override void SetupContainer()
		{
			if (Context == null || PlatformView == null || ContainerView != null)
				return;

			var oldParent = (ViewGroup?)PlatformView.Parent;

			var oldIndex = oldParent?.IndexOfChild(PlatformView);
			oldParent?.RemoveView(PlatformView);

			ContainerView ??= new WrapperView(Context);
			((ViewGroup)ContainerView).AddView(PlatformView);

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(ContainerView, idx);
			else
				oldParent?.AddView(ContainerView);
		}

		protected override void RemoveContainer()
		{
			if (Context == null || PlatformView == null || ContainerView == null || PlatformView.Parent != ContainerView)
				return;

			var oldParent = (ViewGroup?)ContainerView.Parent;

			var oldIndex = oldParent?.IndexOfChild(ContainerView);
			oldParent?.RemoveView(ContainerView);

			((ViewGroup)ContainerView).RemoveAllViews();
			ContainerView = null;

			if (oldIndex is int idx && idx >= 0)
				oldParent?.AddView(PlatformView, idx);
			else
				oldParent?.AddView(PlatformView);
		}
	}
}
