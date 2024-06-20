using System;
using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using Microsoft.Maui.Graphics.Win2D;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Shapes;
using WRect = Windows.Foundation.Rect;
using WSize = Windows.Foundation.Size;

namespace Microsoft.Maui.Platform
{
	internal class BorderedContentPanel : ContentPanel, IBackgroundContainingView
	{
		readonly Path _borderPath;

		IBorderStroke? _borderStroke;

		public BorderedContentPanel()
		{
			_borderPath = new Path();
			Children.Insert(0, _borderPath);

			SizeChanged += ContentPanelSizeChanged;
		}

		internal Path BorderPath => _borderPath;

		FrameworkElement IBackgroundContainingView.BackgroundHost => BorderPath;

		protected override WSize ArrangeOverride(WSize finalSize)
		{
			var actual = base.ArrangeOverride(finalSize);

			_borderPath?.Arrange(new WRect(0, 0, finalSize.Width, finalSize.Height));

			var size = new WSize(Math.Max(0, actual.Width), Math.Max(0, actual.Height));

			// We need to update the clip since the content's position might have changed
			UpdateClip(_borderStroke?.Shape, size.Width, size.Height);

			return size;
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

			_borderPath.UpdatePath(_borderStroke?.Shape, width, height);

			UpdateClip(_borderStroke?.Shape, width, height);
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

			_borderPath.UpdateBorderShape(strokeShape, ActualWidth, ActualHeight);

			var width = ActualWidth;
			var height = ActualHeight;

			if (width <= 0 || height <= 0)
			{
				return;
			}

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

			var clipGeometry = borderShape;

			if (clipGeometry is null)
			{
				return;
			}

			var visual = ElementCompositionPreview.GetElementVisual(Content);
			var compositor = visual.Compositor;

			PathF? clipPath;
			float strokeThickness = (float)(_borderPath?.StrokeThickness ?? 0);
			// The path size should consider the space taken by the border (top and bottom, left and right)
			var pathSize = new Rect(0, 0, width - strokeThickness * 2, height - strokeThickness * 2);

			if (clipGeometry is IRoundRectangle roundedRectangle)
			{
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

			// The clip needs to consider the content's offset in case it is in a different position because of a different alignment.
			geometricClip.Offset = new Vector2(strokeThickness - Content.ActualOffset.X, strokeThickness - Content.ActualOffset.Y);

			visual.Clip = geometricClip;
		}
	}
}
