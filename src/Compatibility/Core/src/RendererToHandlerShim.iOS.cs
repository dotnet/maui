#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using ObjCRuntime;
using UIKit;
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
using PlatformView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim : IPlatformViewHandler
	{
		UIViewController? IPlatformViewHandler.ViewController => VisualElementRenderer?.ViewController;

		protected override PlatformView CreatePlatformView()
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
				PlatformArrange(VisualElementRenderer.Element.Bounds);
			}
		}
	}
}
