using System;
using Gdk;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Point = Microsoft.Maui.Graphics.Point;
using Size = Microsoft.Maui.Graphics.Size;

#pragma warning disable CS0162 // Unreachable code detected

namespace Microsoft.Maui.Platform
{

	public class ScrollView : Gtk.ScrolledWindow
	{
		public ScrollOrientation ScrollOrientation { get; set; }

		internal Func<Graphics.Rect, Graphics.Size>? CrossPlatformArrange { get; set; }
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }

		protected   void _OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);

			double allocation = orientation == Orientation.Vertical ? AllocatedWidth : AllocatedHeight;

			double constraint = Math.Max(allocation, natural_size);
			double hConstraint = AllocatedHeight;
			double wConstraint = AllocatedWidth;
			if (VscrollbarPolicy != PolicyType.Never)
			{
				hConstraint = double.PositiveInfinity;
			}

			if (HscrollbarPolicy != PolicyType.Never)
			{
				wConstraint = double.PositiveInfinity;
			}

			if (CrossPlatformMeasure is { })
			{
				var size = Size.Zero;
				size = orientation == Orientation.Vertical ? 
					new Size(constraint, hConstraint) : 
					new Size(wConstraint, constraint);

				var measure = CrossPlatformMeasure(size.Width, size.Height);
				MaxContentHeight = (int)measure.Height;
				MaxContentWidth = (int)measure.Width;
				//minimum_size = natural_size = (int)(orientation == Orientation.Vertical ? measure.Width : measure.Height);
				;
			}
		}

		protected void OnSizeAllocated_ (Gdk.Rectangle allocation)
		{
			if (CrossPlatformArrange is { } && CrossPlatformMeasure is { })
			{
				var size = allocation.Size.ToSize();
				if (VscrollbarPolicy != PolicyType.Never)
				{
					size.Height = double.PositiveInfinity;
				}

				if (HscrollbarPolicy != PolicyType.Never)
				{
					size.Width = double.PositiveInfinity;
				}

				var measure = CrossPlatformMeasure(size.Width, size.Height);
				CrossPlatformArrange(new Rect(Point.Zero, measure));
			}

			base.OnSizeAllocated(allocation);
		}
	}

}