using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics;
#if MAUI_GRAPHICS_WIN2D
using Microsoft.Maui.Graphics.Win2D;
#else
using Microsoft.Maui.Graphics.Platform;
#endif
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;

namespace Microsoft.Maui.Platform
{
	public class ContentPanel : MauiPanel
	{
		readonly Path? _borderPath;
		IBorderStroke? _borderStroke;
		FrameworkElement? _content;
		PathF _cachedPath = new();
		CompositionPathGeometry? _compositionPathGeometry;

		internal Path? BorderPath => _borderPath;

		internal FrameworkElement? Content
		{
			get => _content;
			set
			{
				var children = Children;

				// Remove the previous content if it exists
				if (_content is not null && children.Contains(_content) && value != _content)
				{
					children.Remove(_content);
				}

				_content = value;

				if (_content is null)
				{
					return;
				}

				if (!children.Contains(_content))
				{
					children.Add(_content);
				}
			}
		}

		internal bool IsInnerPath { get; private set; }

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			var actual = base.ArrangeOverride(finalSize);

			_borderPath?.Arrange(new global::Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));

			var size = new global::Windows.Foundation.Size(Math.Max(0, actual.Width), Math.Max(0, actual.Height));

			// We need to update the clip since the content's position might have changed
			UpdateClip(_borderStroke?.Shape, size.Width, size.Height);

			return size;
		}

		public ContentPanel()
		{
			_borderPath = new Path();
			EnsureBorderPath(containsCheck: false);

			SizeChanged += ContentPanelSizeChanged;
		}

		void ContentPanelSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (_borderPath is null)
			{
				return;
			}

			var width = e.NewSize.Width;
			var height = e.NewSize.Height;

			if (width <= 0 || height <= 0)
			{
				return;
			}

			UpdateBorder(_borderStroke?.Shape);
			UpdateClip(_borderStroke?.Shape, width, height);
		}

		internal void EnsureBorderPath(bool containsCheck = true)
		{
			if (containsCheck)
			{
				var children = Children;

				if (!children.Contains(_borderPath))
				{
					children.Add(_borderPath);
				}
			}
			else
			{
				Children.Add(_borderPath);
			}
		}

		public void UpdateBackground(Paint? background)
		{
			if (_borderPath is null)
			{
				return;
			}

			_borderPath.UpdateBackground(background);
		}

		[Obsolete("Use Microsoft.Maui.Platform.UpdateBorderStroke instead")]
		public void UpdateBorderShape(IShape borderShape)
		{
			UpdateBorder(borderShape);
		}

		internal void UpdateBorderStroke(IBorderStroke borderStroke)
		{
			if (borderStroke is null)
			{
				return;
			}

			_borderStroke = borderStroke;

			if (_borderStroke is null)
			{
				return;
			}

			UpdateBorder(_borderStroke.Shape);
		}

		void UpdateBorder(IShape? strokeShape)
		{
			if (strokeShape is null || _borderPath is null)
			{
				return;
			}

			var width = ActualWidth;
			var height = ActualHeight;

			if (width <= 0 || height <= 0)
			{
				return;
			}

			_cachedPath = _borderPath.UpdatePath(_borderStroke?.Shape, ActualWidth, ActualHeight);
			UpdateClip(strokeShape, width, height);
		}

		void UpdateClip(IShape? borderShape, double width, double height)
		{
			if (Content is null)
			{
				return;
			}

			if (height <= 0 && width <= 0)
			{
				return;
			}

			UpdateCompositionPathGeometry(borderShape, width, height);
			if (_compositionPathGeometry is null)
			{
				return;
			}

			float strokeThickness = (float)(_borderPath?.StrokeThickness ?? 0);
			var visual = ElementCompositionPreview.GetElementVisual(Content);
			var compositor = visual.Compositor;
			var geometricClip = compositor.CreateGeometricClip(_compositionPathGeometry);

			// The clip needs to consider the content's offset in case it is in a different position because of a different alignment.
			geometricClip.Offset = new Vector2(strokeThickness - Content.ActualOffset.X, strokeThickness - Content.ActualOffset.Y);

			visual.Clip = geometricClip;
		}

		void UpdateCompositionPathGeometry(IShape? borderShape, double width, double height)
		{
			if (ActualWidth <= 0 && ActualHeight <= 0)
			{
				return;
			}

			if (borderShape == null)
			{
				return;
			}

			var visual = ElementCompositionPreview.GetElementVisual(this);
			var compositor = visual.Compositor;

			if (borderShape is IRoundRectangle)
			{
				IsInnerPath = true;
			}
			else
			{
				IsInnerPath = false;
			}

			var device = CanvasDevice.GetSharedDevice();
			var geometry = _cachedPath.AsPath(device);
			var path = new CompositionPath(geometry);
			_compositionPathGeometry = compositor.CreatePathGeometry(path);
		}

		internal CompositionSurfaceBrush? GetAlphaMask()
		{
			var compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

			var geoShape = compositor.CreateSpriteShape(_compositionPathGeometry);
			geoShape.FillBrush = compositor.CreateColorBrush(UI.Colors.Black);

			var shapeVisual = compositor.CreateShapeVisual();
			shapeVisual.Shapes.Add(geoShape);

			var visualSurface = compositor.CreateVisualSurface();
			visualSurface.SourceVisual = shapeVisual;

			var surfaceBrush = compositor.CreateSurfaceBrush(visualSurface);
			visualSurface.SourceSize = shapeVisual.Size = RenderSize.ToVector2();
			return surfaceBrush;
		}
	}
}