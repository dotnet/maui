using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.CoreGraphics;
using Microsoft.Maui.Graphics.Native;
using UIKit;

namespace Microsoft.Maui
{
	public partial class VisualDiagnosticsLayer : IVisualDiagnosticsLayer, IDrawable
	{
		public bool DisableUITouchEventPassthrough { get; set; }

		public NativeGraphicsView? VisualDiagnosticsGraphicsView { get; internal set; }

		public void InitializeNativeLayer(IMauiContext context, UIKit.UIWindow nativeLayer)
		{
			if (nativeLayer.RootViewController == null || nativeLayer.RootViewController.View == null)
				return;

			this.VisualDiagnosticsGraphicsView = new NativeGraphicsView(nativeLayer.RootViewController.View.Frame, this, new DirectRenderer());
			if (this.VisualDiagnosticsGraphicsView == null)
			{
				System.Diagnostics.Debug.WriteLine("VisualDiagnosticsLayer: Could not set up touch layer canvas.");
				return;
			}

			var observer = nativeLayer.AddObserver("frame", Foundation.NSKeyValueObservingOptions.OldNew, HandleAction);
			this.VisualDiagnosticsGraphicsView.UserInteractionEnabled = false;
			this.VisualDiagnosticsGraphicsView.BackgroundColor = UIColor.FromWhiteAlpha(1, 0.0f);
			var subviewFrames = nativeLayer.RootViewController.View.Subviews.Select(n => n.Frame).ToArray();
			var height = subviewFrames[1].Height + subviewFrames[2].Height;
			this.Offset = new Rectangle(0, height, 0, 0);
			nativeLayer.RootViewController.View.AddSubview(this.VisualDiagnosticsGraphicsView);
			nativeLayer.RootViewController.View.BringSubviewToFront(this.VisualDiagnosticsGraphicsView);
			this.IsNativeViewInitialized = true;
		}

		private void HandleAction(Foundation.NSObservedChange obj)
		{
			this.Invalidate();
		}

		public void Invalidate()
		{
			this.VisualDiagnosticsGraphicsView?.InvalidateIntrinsicContentSize();
			this.VisualDiagnosticsGraphicsView?.InvalidateDrawable();
		}
	}
}
