using System;
using Microsoft.Maui.Graphics;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using Tizen.UIExtensions.NUI.GraphicsView;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Platform
{
	public partial class WrapperView : ViewGroup
	{
		Lazy<SkiaGraphicsView> _drawableCanvas;
		NView? _content;
		MauiDrawable _mauiDrawable;

		public WrapperView() : base()
		{
			_mauiDrawable = new MauiDrawable();
			_drawableCanvas = new Lazy<SkiaGraphicsView>(() =>
			{
				var view = new SkiaGraphicsView()
				{
					Drawable = _mauiDrawable
				};
				view.Show();
				Children.Add(view);
				view.Lower();
				return view;
			});

			LayoutUpdated += OnLayout;
		}

		public void UpdateBackground(Paint? paint)
		{
			_mauiDrawable.Background = paint;
			_drawableCanvas.Value.Invalidate();
		}

		public void UpdateShape(IShape? shape)
		{
			_mauiDrawable.Shape = shape;
			UpdateDrawableCanvas(false);
		}

		public void UpdateBorder(IBorder border)
		{
			_mauiDrawable.Border = border;
			UpdateShape(border.Shape);
		}

		partial void ShadowChanged()
		{
			_mauiDrawable.Shadow = Shadow;
			UpdateDrawableCanvas(true);
		}

		partial void ClipChanged()
		{
		}

		void UpdateDrawableCanvas(bool isShadowUpdated)
		{
			if (isShadowUpdated)
			{
				UpdateDrawableCanvasGeometry();
			}
			_mauiDrawable.Bounds = GetDrawableBounds();
			_drawableCanvas.Value.Invalidate();
		}

		void OnLayout(object? sender, LayoutEventArgs e)
		{
			if (Content != null)
			{
				Content.UpdateBounds(new Rect(0, 0, Size.Width, Size.Height));
			}

			if (_drawableCanvas.IsValueCreated)
			{
				UpdateDrawableCanvas(true);
			}
		}

		public NView? Content
		{
			get => _content;
			set
			{
				if (_content != value)
				{
					if (_content != null)
					{
						Children.Remove(_content);
						_content = null;
					}
					_content = value;
					if (_content != null)
					{
						Children.Add(_content);
					}
				}
			}
		}

		void UpdateDrawableCanvasGeometry()
		{
			if (_drawableCanvas.IsValueCreated)
			{
				var bounds = new Rect(0, 0, SizeWidth, SizeHeight);
				_drawableCanvas.Value.UpdateBounds(bounds.ToDP().ExpandTo(Shadow).ToPixel());
			}
		}

		Rectangle GetDrawableBounds()
		{
			var drawablePosition = _drawableCanvas.Value.Position;
			var borderThickness = _mauiDrawable.Border?.StrokeThickness ?? 0;
			var padding = (borderThickness / 2).ToScaledPixel();
			var left = - drawablePosition.X + padding;
			var top = - drawablePosition.Y + padding;
			var width = SizeWidth - (padding * 2);
			var height = SizeHeight - (padding * 2);

			return new Rect(left, top, width, height).ToDP();
		}
	}
}
