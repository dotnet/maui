using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim
	{
		protected override PlatformView CreatePlatformView()
		{
			return VisualElementRenderer.ContainerElement;
		}

		IVisualElementRenderer CreateRenderer(IView view)
		{
			return Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view)
										?? new DefaultRenderer();
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			return VisualElementRenderer.GetDesiredSize(widthConstraint, heightConstraint);
		}
	}
}
