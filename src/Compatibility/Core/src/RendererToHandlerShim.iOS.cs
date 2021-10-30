#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using UIKit;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim : INativeViewHandler
	{
		UIViewController? INativeViewHandler.ViewController => VisualElementRenderer?.ViewController;

		protected override NativeView CreateNativeView()
		{
			_ = VisualElementRenderer ?? throw new InvalidOperationException("VisualElementRenderer cannot be null here");

			return VisualElementRenderer.NativeView;
		}

		IVisualElementRenderer CreateRenderer(IView view)
		{
			return Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view)
										?? new DefaultRenderer();
		}

		// TODO ezhart 2021-06-18 This is almost certainly unnecessary; review it along with the OnBatchCommitted method in HandlerToRendererShim
		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			if (property == "Frame" && VisualElementRenderer != null)
			{
				NativeArrange(VisualElementRenderer.Element.Bounds);
			}
		}
	}
}
