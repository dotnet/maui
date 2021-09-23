using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace Microsoft.Maui
{
	sealed class CvViewContainer : UIView
	{
		public CvViewContainer(IMauiContext context)
			: base()
		{
			MauiContext = context;
		}

		public readonly IMauiContext MauiContext;

		public IView VirtualView { get; private set; }

		public UIView NativeView { get; private set; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			NativeView.Frame = Bounds;
		}

		public void SwapView(IView newView)
		{
			if (VirtualView == null || VirtualView.Handler == null || NativeView == null)
			{
				NativeView = newView.ToNative(MauiContext);
				VirtualView = newView;
				AddSubview(NativeView);
			}
			else
			{
				var handler = VirtualView.Handler;
				VirtualView.Handler = null;
				newView.Handler = handler;
				handler.SetVirtualView(newView);
				VirtualView = newView;
			}
		}

		public void SetContainerNeedsLayout()
		{
			VirtualView.InvalidateMeasure();
			VirtualView.InvalidateArrange();

			SetNeedsLayout();
		}
	}
}
