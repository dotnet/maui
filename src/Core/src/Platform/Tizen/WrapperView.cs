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
		Lazy<MauiClipperView> _clipperView;
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

			_clipperView = new Lazy<MauiClipperView>(() =>
			{
				var clipper = new MauiClipperView();
				clipper.Show();
				Children.Add(clipper);
				SetupClipperView(clipper);
				return clipper;
			});

			LayoutUpdated += OnLayout;
		}

		public NView? Content
		{
			get => _content;
			set
			{
				UpdateContent(value, _content);
				_content = value;
			}
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
			if (_clipperView.IsValueCreated || Clip != null)
			{
				_clipperView.Value.Clip = Clip;
			}
		}

		void UpdateDrawableCanvas(bool isShadowUpdated)
		{
			if (isShadowUpdated)
			{
				UpdateDrawableCanvasGeometry();
			}

			_drawableCanvas.Value.Invalidate();
		}

		void OnLayout(object? sender, LayoutEventArgs e)
		{
			if (Content != null)
			{
				Content.UpdateBounds(new Rect(0, 0, Size.Width, Size.Height));
			}

			if (_clipperView.IsValueCreated)
			{
				_clipperView.Value.UpdateBounds(new Rect(0, 0, Size.Width, Size.Height));
			}

			if (_drawableCanvas.IsValueCreated)
			{
				UpdateDrawableCanvas(true);
			}
		}

		void SetupClipperView(MauiClipperView clipper)
		{
			if (_content != null)
			{
				Children.Remove(_content);
				clipper.Add(_content);
			}
		}

		void UpdateContent(NView? newValue, NView? oldValue)
		{
			// remove content from WrapperView
			if (oldValue != null)
			{
				if (_clipperView.IsValueCreated)
				{
					_clipperView.Value.Remove(oldValue);
				}
				else
				{
					Children.Remove(oldValue);
				}
			}

			if (newValue != null)
			{
				if (_clipperView.IsValueCreated)
				{
					_clipperView.Value.Add(newValue);
				}
				else
				{
					Children.Add(newValue);
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
	}
}
