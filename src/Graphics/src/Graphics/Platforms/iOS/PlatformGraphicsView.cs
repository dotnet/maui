using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Graphics.Platform
{
	[Register(nameof(PlatformGraphicsView))]
	public class PlatformGraphicsView : UIView
	{
		private IGraphicsRenderer _renderer;
		private CGColorSpace _colorSpace;
		private IDrawable _drawable;
		private CGRect _lastBounds;

		public PlatformGraphicsView(CGRect frame, IDrawable drawable = null, IGraphicsRenderer renderer = null) : base(frame)
		{
			Drawable = drawable;
			Renderer = renderer;
			BackgroundColor = UIColor.White;
		}

		public PlatformGraphicsView(IDrawable drawable = null, IGraphicsRenderer renderer = null)
		{
			Drawable = drawable;
			Renderer = renderer;
			BackgroundColor = UIColor.White;
		}

		public PlatformGraphicsView(IntPtr aPtr) : base(aPtr)
		{
			BackgroundColor = UIColor.White;
		}

		public IGraphicsRenderer Renderer
		{
			get => _renderer;

			set
			{
				if (_renderer != null)
				{
					_renderer.Drawable = null;
					_renderer.GraphicsView = null;
					_renderer.Dispose();
				}

				_renderer = value ?? new DirectRenderer();

				_renderer.GraphicsView = this;
				_renderer.Drawable = Drawable;
				var bounds = Bounds;
				_renderer.SizeChanged((float)bounds.Width, (float)bounds.Height);
			}
		}

		public IDrawable Drawable
		{
			get => _drawable;
			set
			{
				_drawable = value;
				if (Renderer is IGraphicsRenderer renderer)
				{
					renderer.Drawable = value;
					renderer.Invalidate();
				}
			}
		}

		public void InvalidateDrawable() => Renderer?.Invalidate();

		public void InvalidateDrawable(float x, float y, float w, float h) => Renderer?.Invalidate(x, y, w, h);

		public override void WillMoveToSuperview(UIView newSuperview)
		{
			base.WillMoveToSuperview(newSuperview);

			if (newSuperview == null && Renderer is IGraphicsRenderer renderer)
			{
				renderer.Detached();
			}
		}

		public override void Draw(CGRect dirtyRect)
		{
			base.Draw(dirtyRect);

			if (_drawable == null)
				return;

			var coreGraphics = UIGraphics.GetCurrentContext();

			if (_colorSpace == null)
			{
				_colorSpace = CGColorSpace.CreateDeviceRGB();
			}

			coreGraphics.SetFillColorSpace(_colorSpace);
			coreGraphics.SetStrokeColorSpace(_colorSpace);
			coreGraphics.SetPatternPhase(PatternPhase);

			if (Renderer is IGraphicsRenderer renderer)
			{
				renderer.Draw(coreGraphics, dirtyRect.AsRectangleF());
			}
		}

		public override CGRect Bounds
		{
			get => base.Bounds;

			set
			{
				var newBounds = value;
				if (_lastBounds.Width != newBounds.Width || _lastBounds.Height != newBounds.Height)
				{
					base.Bounds = value;

					if (Renderer is IGraphicsRenderer renderer)
					{
						renderer.SizeChanged((float)newBounds.Width, (float)newBounds.Height);
						renderer.Invalidate();
					}

					_lastBounds = newBounds;
				}
			}
		}

		protected virtual CGSize PatternPhase
		{
			get
			{
				var px = Frame.X;
				var py = Frame.Y;
				return new CGSize(px, py);
			}
		}
	}
}
