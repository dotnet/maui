using System;
using System.Collections.Generic;
using System.Text;
using Android.Views;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Graphics;
using static Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform;
using PlatformView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility
{
	public partial class RendererToHandlerShim
	{
		protected override PlatformView CreatePlatformView()
		{
			_ = VisualElementRenderer ?? throw new InvalidOperationException("VisualElementRenderer cannot be null here");
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
			if (VisualElementRenderer == null)
				return Size.Zero;

			return GetNativeSize(
				VisualElementRenderer, widthConstraint, heightConstraint);
		}

		public override void PlatformArrange(Rect frame)
		{
			// This is a hack to force the shimmed control to actually do layout; without this, some controls won't actually
			// call OnLayout after SetFrame if their sizes haven't changed (e.g., ScrollView)
			// Luckily, measuring with MeasureSpecMode.Exactly is pretty fast, since it just returns the value you give it.
			PlatformView?.Measure(MeasureSpecMode.Exactly.MakeMeasureSpec((int)frame.Width),
				MeasureSpecMode.Exactly.MakeMeasureSpec((int)frame.Height));

			base.PlatformArrange(frame);
		}
	}
}
