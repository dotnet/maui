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

		protected override void SetupContainer() =>
			this.SetupContainerFromHandler((handler) =>
			{
				var view = new WrapperView(handler!.MauiContext!.Context);
				((ViewHandler<TVirtualView, TPlatformView>)handler).ContainerView = view;
				return view;
			});

		protected override void RemoveContainer() =>
			this.RemoveContainerFromHandler((handler) => ((ViewHandler<TVirtualView, TPlatformView>)handler).ContainerView = null);
	}
}
