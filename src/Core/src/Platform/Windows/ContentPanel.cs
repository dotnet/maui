#nullable enable
using System;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Microsoft.Maui.Platform
{
	public class ContentPanel : Panel
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

		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Graphics.Rect, Size>? CrossPlatformArrange { get; set; }

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null || (availableSize.Width * availableSize.Height == 0))
			{
				return base.MeasureOverride(availableSize);
			}

			var measure = CrossPlatformMeasure(availableSize.Width, availableSize.Height);

			return measure.ToPlatform();
		}

		protected override global::Windows.Foundation.Size ArrangeOverride(global::Windows.Foundation.Size finalSize)
		{
			if (CrossPlatformArrange == null)
			{
				return base.ArrangeOverride(finalSize);
			}

			var width = finalSize.Width;
			var height = finalSize.Height;

			var actual = CrossPlatformArrange(new Graphics.Rect(0, 0, width, height));

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

			_borderPath.UpdatePath(_borderShape, ActualWidth, ActualHeight);
			UpdateContent();
			UpdateClip(_borderShape);
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

			_borderPath.UpdateBorderShape(_borderShape, ActualWidth, ActualHeight);
			UpdateContent();
			UpdateClip(_borderShape);
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

		void UpdateClip(IShape? borderShape)
		{
			if (Content == null)
				return;

			var clipGeometry = borderShape;

			if (clipGeometry == null)
				return;

			double width = ActualWidth;
			double height = ActualHeight;

			if (height <= 0 && width <= 0)
				return;

			var visual = ElementCompositionPreview.GetElementVisual(Content);
			var compositor = visual.Compositor;

			var pathSize = new Graphics.Rect(0, 0, width, height);
			var clipPath = clipGeometry.PathForBounds(pathSize);
			var device = CanvasDevice.GetSharedDevice();
			var geometry = clipPath.AsPath(device);

			var path = new CompositionPath(geometry);
			var pathGeometry = compositor.CreatePathGeometry(path);
			var geometricClip = compositor.CreateGeometricClip(pathGeometry);

			visual.Clip = geometricClip;
		}
	}
}