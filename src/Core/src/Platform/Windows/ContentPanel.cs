#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using WDoubleCollection = Microsoft.UI.Xaml.Media.DoubleCollection;
using WPenLineCap = Microsoft.UI.Xaml.Media.PenLineCap;
using WPenLineJoin = Microsoft.UI.Xaml.Media.PenLineJoin;

namespace Microsoft.Maui.Platform
{
	public class ContentPanel : Panel
	{
		internal Func<double, double, Size>? CrossPlatformMeasure { get; set; }
		internal Func<Graphics.Rectangle, Size>? CrossPlatformArrange { get; set; }

		protected override global::Windows.Foundation.Size MeasureOverride(global::Windows.Foundation.Size availableSize)
		{
			if (CrossPlatformMeasure == null)
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

			var actual = CrossPlatformArrange(new Graphics.Rectangle(0, 0, width, height));

			return new global::Windows.Foundation.Size(actual.Width, actual.Height);
		}

		public ContentPanel() 
		{
			_borderPath = new Path();
			EnsureBorderPath();
			
			SizeChanged += ContentPanelSizeChanged;
		}

		private void ContentPanelSizeChanged(object sender, UI.Xaml.SizeChangedEventArgs e)
		{
			UpdatePath();
		}

		internal void EnsureBorderPath() 
		{
			if (!Children.Contains(_borderPath))
			{
				Children.Add(_borderPath);
			}
		}

		readonly Path? _borderPath;
		IShape? _borderShape;

		public void UpdateBackground(Paint? background)
		{
			if (_borderPath == null)
				return;

			_borderPath.Fill = background?.ToPlatform();
		}

		public void UpdateStroke(Paint borderBrush)
		{
			if (_borderPath == null)
				return;

			_borderPath.Stroke = borderBrush.ToPlatform();
		}

		public void UpdateStrokeThickness(double borderWidth)
		{
			if (_borderPath == null)
				return;

			_borderPath.StrokeThickness = borderWidth;
		}

		public void UpdateStrokeDashPattern(float[]? borderDashArray)
		{
			if (_borderPath == null)
				return;

			if (_borderPath.StrokeDashArray != null)
				_borderPath.StrokeDashArray.Clear();

			if (borderDashArray != null && borderDashArray.Length > 0)
			{
				if (_borderPath.StrokeDashArray == null)
					_borderPath.StrokeDashArray = new WDoubleCollection();

				double[] array = new double[borderDashArray.Length];
				borderDashArray.CopyTo(array, 0);

				foreach (double value in array)
				{
					_borderPath.StrokeDashArray.Add(value);
				}
			}
		}

		public void UpdateBorderShape(IShape borderShape)
		{
			_borderShape = borderShape;
			UpdatePath();
		}

		public void UpdateBorderDashOffset(double borderDashOffset)
		{
			if (_borderPath == null)
				return;

			_borderPath.StrokeDashOffset = borderDashOffset;
		}

		public void UpdateStrokeMiterLimit(double strokeMiterLimit)
		{
			if (_borderPath == null)
				return;

			_borderPath.StrokeMiterLimit = strokeMiterLimit;
		}

		public void UpdateStrokeLineCap(LineCap strokeLineCap)
		{
			if (_borderPath == null)
				return;

			WPenLineCap wLineCap = WPenLineCap.Flat;

			switch (strokeLineCap)
			{
				case LineCap.Butt:
					wLineCap = WPenLineCap.Flat;
					break;
				case LineCap.Square:
					wLineCap = WPenLineCap.Square;
					break;
				case LineCap.Round:
					wLineCap = WPenLineCap.Round;
					break;
			}

			_borderPath.StrokeStartLineCap = _borderPath.StrokeEndLineCap = wLineCap;
		}

		public void UpdateStrokeLineJoin(LineJoin strokeLineJoin)
		{
			if (_borderPath == null)
				return;

			WPenLineJoin wLineJoin = WPenLineJoin.Miter;

			switch (strokeLineJoin)
			{
				case LineJoin.Miter:
					wLineJoin = WPenLineJoin.Miter;
					break;
				case LineJoin.Bevel:
					wLineJoin = WPenLineJoin.Bevel;
					break;
				case LineJoin.Round:
					wLineJoin = WPenLineJoin.Round;
					break;
			}

			_borderPath.StrokeLineJoin = wLineJoin;
		}

		void UpdatePath()
		{
			if (_borderShape == null)
				return;

			var strokeThickness = _borderPath?.StrokeThickness ?? 0;

			var width = ActualWidth;
			var height = ActualHeight;

			if (width <= 0 || height <= 0)
				return;

			var pathSize = new Graphics.Rectangle(0, 0, width + strokeThickness, height + strokeThickness);
			var shapePath = _borderShape.PathForBounds(pathSize);
			var geometry = shapePath.AsPathGeometry();

			if (_borderPath != null)
			{
				_borderPath.Data = geometry;
				_borderPath.RenderTransform = new TranslateTransform() { X = -(strokeThickness/2), Y = -(strokeThickness/2) };
			}
		}
	}
}
