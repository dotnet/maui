using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat.Platform;
using NativeView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility
{
    public partial class RendererToHandlerShim
    {
		protected override NativeView CreateNativeView()
		{
			return VisualElementRenderer.View;
		}

		IVisualElementRenderer CreateRenderer(IView view)
		{
			if (Context != null)
			{
				var renderer = Internals.Registrar.Registered.GetHandlerForObject<IVisualElementRenderer>(view, Context)
										   ?? new DefaultRenderer(Context);
				return renderer;
			}

			return null;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return GetNativeSize(
				VisualElementRenderer, widthConstraint, heightConstraint);
		}

		public override void NativeArrange(Rectangle frame)
		{
			// This is a hack to force the shimmed control to actually do layout; without this, some controls won't actually
			// call OnLayout after SetFrame if their sizes haven't changed (e.g., ScrollView)
			// Luckily, measuring with MeasureSpecMode.Exactly is pretty fast, since it just returns the value you give it.
			NativeView?.Measure(MeasureSpecMode.Exactly.MakeMeasureSpec((int)frame.Width),
				MeasureSpecMode.Exactly.MakeMeasureSpec((int)frame.Height));

			base.NativeArrange(frame);
		}
	}
}
