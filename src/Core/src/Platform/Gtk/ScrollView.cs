using System;
using Gtk;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Platform
{

	public class ScrollView : Gtk.ScrolledWindow
	{

		public ScrollOrientation ScrollOrientation { get; set; }

		internal Func<Graphics.Rect, Graphics.Size>? CrossPlatformArrange { get; set; }
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);
		}
	}

}