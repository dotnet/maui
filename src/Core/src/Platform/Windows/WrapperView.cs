using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using WDoubleCollection = Microsoft.UI.Xaml.Media.DoubleCollection;

namespace Microsoft.Maui
{
	public partial class WrapperView : Grid
	{
		readonly Path? _borderShape;
		FrameworkElement? _child;

		public WrapperView()
		{
			_borderShape = new Path();

			Children.Add(_borderShape);
		}

		public FrameworkElement? Child
		{
			get { return _child; }
			internal set
			{
				if (_child != null)
					Children.Remove(_child);

				if (value == null)
					return;

				_child = value;
				Children.Add(_child);
			}
		}

		public void SetBorderShape(IShape borderShape)
		{
			if (_borderShape == null)
				return;

			var pathSize = new Graphics.Rectangle(0, 0, Width, Height);
			var shapePath = borderShape.PathForBounds(pathSize);
			var geometry = shapePath.AsPathGeometry();

			_borderShape.Data = geometry;
		}

		public void SetBorderBrush(Paint borderBrush)
		{
			if (_borderShape == null)
				return;

			_borderShape.Stroke = borderBrush.ToNative();
		}

		public void SetBorderWidth(double borderWidth)
		{
			if (_borderShape == null)
				return;

			_borderShape.StrokeThickness = borderWidth;
		}

		public void SetBorderDashArray(DoubleCollection borderDashArray)
		{
			if (_borderShape == null)
				return;

			if (_borderShape.StrokeDashArray != null)
				_borderShape.StrokeDashArray.Clear();

			if (borderDashArray != null && borderDashArray.Count > 0)
			{
				if (_borderShape.StrokeDashArray == null)
					_borderShape.StrokeDashArray = new WDoubleCollection();

				double[] array = new double[borderDashArray.Count];
				borderDashArray.CopyTo(array, 0);

				foreach (double value in array)
				{
					_borderShape.StrokeDashArray.Add(value);
				}
			}
		}

		public void SetBorderDashOffset(double borderDashOffset)
		{
			if (_borderShape == null)
				return;

			_borderShape.StrokeDashOffset = borderDashOffset;
		}
	}
}