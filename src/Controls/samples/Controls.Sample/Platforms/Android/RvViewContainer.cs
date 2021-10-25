using System;
using AContext = Android.Content.Context;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;
using Microsoft.Maui;
using Microsoft.Maui.HotReload;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	sealed class RvViewContainer : Android.Widget.FrameLayout // AViewGroup
	{
		public RvViewContainer(IMauiContext context)
			: base(context.Context ?? throw new ArgumentNullException($"{nameof(context.Context)}"))
		{
			MauiContext = context;
			Id = AView.GenerateViewId();
		}

		public readonly IMauiContext MauiContext;

		public IView VirtualView { get; private set; }

		public AView NativeView { get; private set; }

		public void SwapView(IView newView)
		{
			if (VirtualView == null || VirtualView.Handler == null || NativeView == null)
			{
				NativeView = newView.ToNative(MauiContext);
				VirtualView = newView;
				AddView(NativeView);
			}
			else
			{
				var handler = VirtualView.Handler;
				newView.Handler = handler;
				handler.SetVirtualView(newView);
				VirtualView = newView;
			}

			VirtualView.InvalidateMeasure();
			VirtualView.InvalidateArrange();

			//Invalidate();
		}

		float? displayScale;
		float DisplayScale
		{
			get
			{
				if (!displayScale.HasValue)
					displayScale = MauiContext?.Context?.Resources?.DisplayMetrics?.Density;

				return displayScale ?? 1;
			}
		}
	}
}