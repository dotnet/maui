using Gdk;
using System;
using Cairo;

namespace Xamarin.Forms.Platform.GTK.Controls
{
	public class ImageControl : Gtk.HBox, IDesiredSizeProvider
	{
		private Gtk.Image _image;
		private Pixbuf _original;
		private ImageAspect _aspect;

		private double _scaleX;
		private double _scaleY;
		private double _scale;

		private double _rotation;

		private Gdk.Rectangle _lastAllocation = Gdk.Rectangle.Zero;

		public ImageControl()
		{
			_aspect = ImageAspect.AspectFill;
			_scaleX = 1.0;
			_scaleY = 1.0;
			_scale = 1.0;
			_rotation = 0.0;
			BuildImageControl();
		}

		public ImageAspect Aspect
		{
			get
			{
				return _aspect;
			}

			set
			{
				_aspect = value;
				QueueDraw();
			}
		}

		public Pixbuf Pixbuf
		{
			get
			{
				return _image.Pixbuf;
			}
			set
			{
				_lastAllocation = Gdk.Rectangle.Zero;
				_original = value;
				_image.Pixbuf = value;
			}
		}

		public double ScaleX
		{
			get
			{
				return _scaleX;
			}
			set
			{
				_scaleX = value;
				UpdateScaleAndRotation();
			}
		}

		public double ScaleY
		{
			get
			{
				return _scaleY;
			}
			set
			{
				_scaleY = value;
				UpdateScaleAndRotation();
			}
		}

		public double Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scaleX = value;
				_scaleY = value;
				_scale = value;
				UpdateScaleAndRotation();
			}
			
		}

		public double Rotation
		{
			get
			{
				return _rotation;
			}
			set
			{
				_rotation = value;
				UpdateScaleAndRotation();
			}
		}

		public void SetAlpha(double opacity)
		{
			if (_image != null && _original != null)
			{
				_image.Pixbuf = Pixbuf.AddAlpha(
					true,
					((byte)(255 * opacity)),
					((byte)(255 * opacity)),
					((byte)(255 * opacity)));
			}
		}

		public Gdk.Size GetDesiredSize()
		{
			return _original != null
				? new Gdk.Size(_original.Width, _original.Height)
				: Gdk.Size.Empty;
		}

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			if (_image.Pixbuf != null && _lastAllocation != allocation)
			{
				_lastAllocation = allocation;
				UpdatePixBufWithAllocation(allocation);
			}
		}

		private void BuildImageControl()
		{
			CanFocus = true;

			_image = new Gtk.Image();

			PackStart(_image, true, true, 0);
		}

		private static Pixbuf GetAspectFitPixBuf(Pixbuf original, Gdk.Rectangle allocation)
		{
			var widthRatio = (float)allocation.Width / original.Width;
			var heigthRatio = (float)allocation.Height / original.Height;

			var fitRatio = Math.Min(widthRatio, heigthRatio);
			var finalWidth = (int)(original.Width * fitRatio);
			var finalHeight = (int)(original.Height * fitRatio);

			return original.ScaleSimple(finalWidth, finalHeight, InterpType.Bilinear);
		}

		private static Pixbuf GetAspectFillPixBuf(Pixbuf original, Gdk.Rectangle allocation)
		{
			var widthRatio = (float)allocation.Width / original.Width;
			var heigthRatio = (float)allocation.Height / original.Height;

			var fitRatio = Math.Max(widthRatio, heigthRatio);
			var finalWidth = (int)(original.Width * fitRatio);
			var finalHeight = (int)(original.Height * fitRatio);

			return original.ScaleSimple(finalWidth, finalHeight, InterpType.Bilinear);
		}

		private static Pixbuf GetFillPixBuf(Pixbuf original, Gdk.Rectangle allocation)
		{
			return original.ScaleSimple(allocation.Width, allocation.Height, InterpType.Bilinear);
		}

		private void UpdatePixBufWithAllocation(Gdk.Rectangle allocation)
		{
			var srcWidth = _original.Width;
			var srcHeight = _original.Height;

			Pixbuf newPixBuf = null;

			// Differents modes in which the image will be scaled to fit the display area.
			switch (Aspect)
			{
				case ImageAspect.AspectFit:
					newPixBuf = GetAspectFitPixBuf(_original, allocation);
					break;
				case ImageAspect.AspectFill:
					newPixBuf = GetAspectFillPixBuf(_original, allocation);
					break;
				case ImageAspect.Fill:
					newPixBuf = GetFillPixBuf(_original, allocation);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(Aspect));
			}

			if (newPixBuf != null)
			{
				_image.Pixbuf = newPixBuf;
				newPixBuf.Dispose();	// Important: Image should adapt to window size. To maintain memory consuption, we make Pixbuf dispose (Unref is deprecated).
				System.GC.Collect();
			}
		}

		private void UpdateScaleAndRotation()
		{
			if (_image != null && _original != null)
			{
				Pixbuf rotated;

				if(_rotation != 0.0)
				{
					ImageSurface surface = new ImageSurface(Format.Argb32, _original.Width, _original.Height);
					Context ctx = new Context(surface);

					ctx.Translate(_original.Width / 2.0, _original.Height / 2.0);
					double radians = _rotation * (Math.PI / 180.0);
					ctx.Rotate(radians);
					ctx.Translate(-_original.Width / 2.0, -_original.Height / 2.0);
					CairoHelper.SetSourcePixbuf(ctx, _original, 0, 0);
					ctx.Paint();

					rotated = GetPixbufFromImageSurface(surface, surface.Width, surface.Height);

					surface.Dispose();
					ctx.GetTarget().Dispose();
					ctx.Dispose();
				}
				else
				{
					rotated = _original;
				}

				if(_scaleX != 0.0 || _scaleY != 0.0)
				{
					_image.Pixbuf = rotated.ScaleSimple((int)(rotated.Width * _scale), (int)(rotated.Height * _scale), InterpType.Bilinear);
				}
				else
				{
					_image.Pixbuf = rotated;
				}
			}
			QueueDraw();
		}

		private Pixbuf GetPixbufFromImageSurface(ImageSurface surface, int width, int height)
		{
			/*
			 *  This is a simplified implementation of gdk_pixbuf_get_from_surface()
			 *  which is not supported by gtk-sharp.
			 * 
			 *  See https://code.woboq.org/gtk/gtk/gdk/gdkpixbuf-drawable.c.html
			 *  for original implementation.
			 */
			Pixbuf dest = new Pixbuf(Gdk.Colorspace.Rgb, true, 8, width, height);
			ConvertAlpha(dest.Pixels, dest.Rowstride, surface.Data, surface.Stride, 0, 0, width, height);
			return dest;
		}

		private void ConvertAlpha(IntPtr destData, int destStride, byte[] srcData, int srcStride, int srcX, int srcY, int width, int height)
		{
			int srcDataIndex = srcStride * srcY + srcX * 4;

			for(int y = 0; y < height; y++)
			{
				for(int x = 0; x < width; x++)
				{
					byte a = srcData[srcDataIndex + (x * 4) + 3];

					if(a == 0)
					{
						System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 0), 0);
						System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 1), 0);
						System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 2), 0);
					}
					else
					{
						byte b = (byte)((srcData[srcDataIndex + (x * 4) + 0] * 255 + a / 2) / a);
						byte g = (byte)((srcData[srcDataIndex + (x * 4) + 1] * 255 + a / 2) / a);
						byte r = (byte)((srcData[srcDataIndex + (x * 4) + 2] * 255 + a / 2) / a);

						System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 0), r);
						System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 1), g);
						System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 2), b);
					}
					System.Runtime.InteropServices.Marshal.WriteByte(IntPtr.Add(destData, x * 4 + 3), a);
				}

				srcDataIndex += srcStride;
				destData = IntPtr.Add(destData, destStride);
			}
		}
	}

	public enum ImageAspect
	{
		AspectFit,
		AspectFill,
		Fill
	}
}

