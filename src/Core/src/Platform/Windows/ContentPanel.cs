#nullable enable
using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Microsoft.Maui.Platform
{
	public class ContentPanel : MauiPanel
	{
		readonly Path? _borderPath;
		IShape? _borderShape;
		FrameworkElement? _content;

		internal Path? BorderPath => _borderPath;

		internal FrameworkElement? Content
		{
			get => _content;
			set
			{
				_content = value;
				AddContent(_content);
			}
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			var actual = base.ArrangeOverride(finalSize);

			_borderPath?.Arrange(new global::Windows.Foundation.Rect(0, 0, finalSize.Width, finalSize.Height));

			return new global::Windows.Foundation.Size(Math.Max(0, actual.Width), Math.Max(0, actual.Height));
		}

		public ContentPanel()
		{
			_borderPath = new Path();
			EnsureBorderPath();

			SizeChanged += ContentPanelSizeChanged;
		}

		void ContentPanelSizeChanged(object sender, UI.Xaml.SizeChangedEventArgs e)
		{
			if (_borderPath == null)
				return;

			var width = e.NewSize.Width;
			var height = e.NewSize.Height;

			if (width <= 0 || height <= 0)
			{
				return;
			}

			_borderPath.UpdatePath(_borderShape, width, height);
			UpdateContent();
			UpdateClip(_borderShape, width, height);
		}

		internal void EnsureBorderPath()
		{
			if (!Children.Contains(_borderPath))
			{
				Children.Add(_borderPath);
			}
		}

		public void UpdateBackground(Paint? background)
		{
			if (_borderPath == null)
				return;

			_borderPath.UpdateBackground(background);
		}

		public void UpdateBorderShape(IShape borderShape)
		{
			_borderShape = borderShape;

			if (_borderPath == null)
				return;

			var width = ActualWidth;
			var height = ActualHeight;

			_borderPath.UpdateBorderShape(_borderShape, width, height);
			UpdateContent();
			UpdateClip(_borderShape, width, height);
		}

		void AddContent(FrameworkElement? content)
		{
			if (content == null)
				return;

			if (!Children.Contains(_content))
				Children.Add(_content);
		}

		void UpdateContent()
		{
			if (Content == null || _borderPath == null)
				return;

			var strokeThickness = _borderPath.StrokeThickness;

			Content.RenderTransform = new TranslateTransform() { X = -strokeThickness, Y = -strokeThickness };
		}

		void UpdateClip(IShape? borderShape, double width, double height)
		{
			if (Content == null)
				return;
			
			if (height <= 0 && width <= 0)
				return;

			var clipGeometry = borderShape;

			if (clipGeometry == null)
				return;

			var visual = ElementCompositionPreview.GetElementVisual(Content);
			var compositor = visual.Compositor;

			var pathSize = new Graphics.Rect(0, 0, width, height);

			PathF? clipPath;

			if (clipGeometry is IRoundRectangle roundedRectangle)
			{
				float strokeThickness = (float)(_borderPath?.StrokeThickness ?? 0);
				clipPath = roundedRectangle.InnerPathForBounds(pathSize, strokeThickness / 2);
			}
			else
			{
				clipPath = clipGeometry.PathForBounds(pathSize);
			}

			var device = CanvasDevice.GetSharedDevice();
			var geometry = clipPath.AsPath(device);

			var path = new CompositionPath(geometry);
			var pathGeometry = compositor.CreatePathGeometry(path);
			var geometricClip = compositor.CreateGeometricClip(pathGeometry);

			visual.Clip = geometricClip;
		}
	}
}