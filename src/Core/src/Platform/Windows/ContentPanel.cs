#nullable enable
using System;
using Microsoft.Graphics.Canvas;
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
		IBorderStroke? _borderStroke;
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

		internal bool IsInnerPath { get; private set; }

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
			if (_borderPath is null)
				return;

			var width = e.NewSize.Width;
			var height = e.NewSize.Height;

			if (width <= 0 || height <= 0)
				return;

			_borderPath.UpdatePath(_borderStroke?.Shape, width, height);
			UpdateClip(_borderStroke?.Shape, width, height);
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

		[Obsolete("Use Microsoft.Maui.Platform.UpdateBorderStroke instead")]
		public void UpdateBorderShape(IShape borderShape)
		{
			UpdateBorder(borderShape);
		}

		internal void UpdateBorderStroke(IBorderStroke borderStroke)
		{
			if (borderStroke is null)
				return;

			_borderStroke = borderStroke;

			if (_borderStroke is null)
				return;

			UpdateBorder(_borderStroke.Shape);
		}

		void UpdateBorder(IShape? strokeShape)
		{
			if (strokeShape is null || _borderPath is null)
				return;

			_borderPath.UpdateBorderShape(strokeShape, ActualWidth, ActualHeight);

			var width = ActualWidth;
			var height = ActualHeight;

			if (width <= 0 || height <= 0)
				return;

			UpdateClip(strokeShape, width, height);
		}

		void AddContent(FrameworkElement? content)
		{
			if (content == null)
				return;

			if (!Children.Contains(_content))
				Children.Add(_content);
		}

		void UpdateClip(IShape? borderShape, double width, double height)
		{
			if (Content is null)
				return;

			if (height <= 0 && width <= 0)
				return;

			var clipGeometry = borderShape;

			if (clipGeometry is null)
				return;

			var visual = ElementCompositionPreview.GetElementVisual(Content);
			var compositor = visual.Compositor;


			var pathSize = new Graphics.Rect(0, 0, width, height);
			PathF? clipPath;

			if (clipGeometry is IRoundRectangle roundedRectangle)
			{
				float strokeThickness = (float)(_borderPath?.StrokeThickness ?? 0);
				clipPath = roundedRectangle.InnerPathForBounds(pathSize, strokeThickness / 2);
				IsInnerPath = true;
			}
			else
			{
				clipPath = clipGeometry.PathForBounds(pathSize);
				IsInnerPath = false;
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