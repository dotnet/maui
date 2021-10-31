#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Controls
{
	public partial class HighlightLayer
	{
		HashSet<iOSView> Views = new HashSet<iOSView>();

		public bool AddHighlight(Maui.IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return false;
			var selectionLayer = new UIView(new CGRect(CGPoint.Empty, nativeView.Bounds.Size));
			selectionLayer.BackgroundColor = UIColor.Clear;
			selectionLayer.Layer.BorderColor = UIColor.Red.CGColor;
			selectionLayer.Layer.BorderWidth = 1;
			nativeView.InvokeOnMainThread(() => nativeView.AddSubview(selectionLayer));
			return this.Views.Add(new iOSView(view, nativeView));
		}

		public bool RemoveHighlight(Maui.IView view)
		{
			var aview = this.Views.FirstOrDefault(n => n.view == view);
			if (aview != null)
			{
				aview.highlightedView.InvokeOnMainThread(() => {
					aview.highlightedView.RemoveFromSuperview();
					aview.highlightedView.Dispose();
				});
				return this.Views.Remove(aview);
			}
			return false;
		}

		public void ClearHighlights()
		{
			foreach(var selectionLayer in Views)
			{
				selectionLayer.highlightedView.InvokeOnMainThread(() => {
					selectionLayer.highlightedView.RemoveFromSuperview();
					selectionLayer.highlightedView.Dispose();
				}); 
			}
			this.Views.Clear();
		}

		internal class iOSView
		{
			public Maui.IView view;
			public UIView highlightedView;

			public iOSView(Maui.IView view, UIView uiview)
			{
				this.view = view;
				this.highlightedView = uiview;
			}
		}
	}
}
