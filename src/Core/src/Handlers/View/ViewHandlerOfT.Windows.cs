#nullable enable
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Handlers
{
	public partial class ViewHandler<TVirtualView, TNativeView> : FrameworkElementHandler<TVirtualView, TNativeView>
	{
		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (NativeView == null || VirtualView == null)
				return Size.Zero;

			if (widthConstraint < 0 || heightConstraint < 0)
				return Size.Zero;

			var measureConstraint = new Windows.Foundation.Size(widthConstraint, heightConstraint);

			NativeView.Measure(measureConstraint);

			return new Size(NativeView.DesiredSize.Width, NativeView.DesiredSize.Height);
		}
	}
}
