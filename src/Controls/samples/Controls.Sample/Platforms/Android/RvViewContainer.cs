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

		//protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		//{
		//	var specMode = MeasureSpec.GetMode(widthMeasureSpec);
		//	var specSize = MeasureSpec.GetSize(widthMeasureSpec);

		//	base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

		//	var size = VirtualView.Measure(specSize / DisplayScale, double.PositiveInfinity);

		//	var nativeWidth = Math.Max(specSize, Context.ToPixels(size.Width));
		//	var nativeHeight = 300; // Context.ToPixels(size.Height);

		//	SetMeasuredDimension((int)nativeWidth, (int)nativeHeight);

		//	Console.WriteLine($"OnMeasure: {VirtualView.GetType().Name}: {nativeWidth}x{nativeHeight}");
		//}

		//protected override void OnLayout(bool changed, int l, int t, int r, int b)
		//{
		//	if (changed)
		//	{
		//		var deviceIndependentLeft = Context.FromPixels(l);
		//		var deviceIndependentTop = Context.FromPixels(t);
		//		var deviceIndependentRight = Context.FromPixels(r);
		//		var deviceIndependentBottom = Context.FromPixels(b);

		//		var destination = Rectangle.FromLTRB(deviceIndependentLeft, deviceIndependentTop,
		//			deviceIndependentRight, deviceIndependentBottom);

		//		Console.WriteLine($"OnLayout: {deviceIndependentLeft}, {deviceIndependentTop}, {deviceIndependentRight}, {deviceIndependentBottom}");

		//		VirtualView.Arrange(destination);

		//		var vf = VirtualView.Frame;

		//		Console.WriteLine($"VirtualFrame: {vf.Left}, {vf.Top}, {vf.Right}, {vf.Bottom}");
		//		(VirtualView.Handler as INativeViewHandler)?.NativeArrange(VirtualView.Frame);
		//	}

		//	// _renderer.UpdateLayout();
		//}
	}
}