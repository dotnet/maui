using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using static Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform;
using NativeView = UIKit.UIView;

namespace Microsoft.Maui.Controls.Compatibility
{
    public partial class RendererToHandlerShim
	{
		protected override NativeView CreateNativeView()
		{
			return VisualElementRenderer.NativeView;
		}

		IVisualElementRenderer CreateRenderer(IView view)
		{
			return Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view)
										?? new DefaultRenderer();
		}

		public override void UpdateValue(string property)
		{
			base.UpdateValue(property);
			if (property == "Frame")
			{
				NativeArrange(VisualElementRenderer.Element.Bounds);
			}
		}
	}
}
