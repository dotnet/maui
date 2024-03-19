using System;
using System.Net.Mime;
using Cairo;
using Gtk;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform.Gtk;
using Rectangle = Gdk.Rectangle;

namespace Microsoft.Maui.Platform
{
	// https://docs.gtk.org/gtk3/class.Image.html 

	// GtkImage has nothing like Aspect; maybe an ownerdrawn class is needed 
	// could be: https://docs.gtk.org/gtk3/class.DrawingArea.html
	// or Microsoft.Maui.Graphics.Platform.Gtk.GtkGraphicsView

	public class ImageView : Gtk.DrawingArea
	{
		public ImageView()
		{
			CanFocus = true;
			AddEvents((int)Gdk.EventMask.AllEventsMask);
		}

		Gdk.Pixbuf? Pixbuf;

		public Gdk.Pixbuf? Image
		{
			get => Pixbuf;
			set
			{
				Pixbuf = value;
				QueueResize();
			}
		}

		Aspect _aspect;

		public Aspect Aspect
		{
			get => _aspect;
			set
			{
				_aspect = value;
			}
		}

		public static Size Measure(Aspect aspect, Size desiredSize, double? widthConstraint, double? heightConstraint)
		{
			double desiredAspect = desiredSize.Width / desiredSize.Height;
			if (widthConstraint is not { } && heightConstraint is { })
				widthConstraint = desiredSize.Width * heightConstraint / desiredSize.Height;
			if (widthConstraint is { } && heightConstraint is not { })
				heightConstraint = desiredSize.Height * widthConstraint / desiredSize.Width;
			if (widthConstraint is not { } && heightConstraint is not { })
			{
				widthConstraint = desiredSize.Width;
				heightConstraint = desiredSize.Height;
			}

			double constraintAspect = widthConstraint!.Value / heightConstraint!.Value;

			double desiredWidth = desiredSize.Width;
			double desiredHeight = desiredSize.Height;

			if (desiredWidth == 0 || desiredHeight == 0)
				return new SizeRequest(new Size(0, 0));

			double width = desiredWidth;
			double height = desiredHeight;
			if (constraintAspect > desiredAspect)
			{
				// constraint area is proportionally wider than image
				switch (aspect)
				{
					case Aspect.AspectFit:
						width = desiredWidth * (height / desiredHeight);
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.AspectFill:
						height = heightConstraint!.Value;
						width = desiredWidth * (height / desiredHeight);
						break;
					case Aspect.Fill:
						width = Math.Min(desiredWidth, widthConstraint!.Value);
						height = desiredHeight * (width / desiredWidth);
						break;
				}
			}
			else if (constraintAspect < desiredAspect)
			{
				// constraint area is proportionally taller than image
				switch (aspect)
				{
					case Aspect.AspectFit:
						width = Math.Min(desiredWidth, widthConstraint!.Value);
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.AspectFill:
						width = widthConstraint!.Value;
						height = desiredHeight * (width / desiredWidth);
						break;
					case Aspect.Fill:
						height = Math.Min(desiredHeight, heightConstraint!.Value);
						width = desiredWidth * (height / desiredHeight);
						break;
				}
			}
			else
			{
				// constraint area is same aspect as image
				width = Math.Min(desiredWidth, widthConstraint!.Value);
				height = desiredHeight * (width / desiredWidth);
			}

			return new Size(width, height);
		}

		protected override bool OnDrawn(Context cr)
		{
			var allocation = Allocation;
			var stc = this.StyleContext;
			// stc.RenderBackground(cr, 0, 0, a.Width, a.Height);

			if (Image is not { } image)
				return true;

			// HACK: Gtk sends sometimes a draw event while the widget reallocates.
			//       In that case we would draw in the wrong area, which may lead to artifacts
			//       if no other widget updates it. Alternative: we could clip the
			//       allocation bounds, but this may have other issues.
			if (allocation.Width == 1 && allocation.Height == 1 && allocation.X == -1 && allocation.Y == -1) // the allocation coordinates on reallocation
				return base.OnDrawn(cr);


			var imgSize = Measure(Aspect, new Size(image.Width, image.Height), allocation.Width, allocation.Height);
			var x = (allocation.Width - imgSize.Width) / 2;
			var y = (allocation.Height - imgSize.Height) / 2;

			if (false)
#pragma warning disable CS0162 // Unreachable code detected
			{
				using var region = Window.VisibleRegion;
				using var frame = Window.BeginDrawFrame(region);
				using var crr = frame.CairoContext;
				crr.DrawPixbuf(image, x, y, allocation.Width, allocation.Height);
				Window.EndDrawFrame(frame);
			}
#pragma warning restore CS0162 // Unreachable code detected
			else
			{
				cr.DrawPixbuf(image, x, y, imgSize.Width, imgSize.Height);
			}

			return false;
		}

		protected override void OnAdjustSizeRequest(Orientation orientation, out int minimum_size, out int natural_size)
		{
			base.OnAdjustSizeRequest(orientation, out minimum_size, out natural_size);

			if (Image is not { })
				return;
		}

		protected override void OnRealized()
		{
			base.OnRealized();
		}

		protected override SizeRequestMode OnGetRequestMode()
		{
			return SizeRequestMode.HeightForWidth;
		}

		protected override void OnSizeAllocated(Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);
		}

		protected override void OnGetPreferredHeightForWidth(int width, out int minimum_height, out int natural_height)
		{
			base.OnGetPreferredHeightForWidth(width, out minimum_height, out natural_height);

			if (Image is { })
			{
				var imgSize = new Size(Image.Width, Image.Height);
				var size = Measure(Aspect, imgSize, width, default);

				minimum_height = (int)size.Height;
				natural_height = Math.Max(Image.Height, minimum_height);
			}
		}

		protected override void OnGetPreferredWidthForHeight(int height, out int minimum_width, out int natural_width)
		{
			base.OnGetPreferredWidthForHeight(height, out minimum_width, out natural_width);

			if (Image is { })
			{
				var imgSize = new Size(Image.Width, Image.Height);
				var size = Measure(Aspect, imgSize, default, height);
				minimum_width = (int)size.Width;
				natural_width = Math.Max(Image.Width, minimum_width);
			}
		}
	}
}