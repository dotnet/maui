using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using WDoubleCollection = Microsoft.UI.Xaml.Media.DoubleCollection;

namespace Microsoft.Maui
{
	public partial class WrapperView : Grid
	{
		readonly Path? _borderPath;
		IShape? _borderShape;
		FrameworkElement? _child;

		public WrapperView()
		{
			_borderPath = new Path();

			Children.Add(_borderPath);
		}

		public FrameworkElement? Child
		{
			get { return _child; }
			internal set
			{
				if (_child != null)
				{
					_child.SizeChanged -= OnChildSizeChanged;
					Children.Remove(_child);
				}

				if (value == null)
					return;

				_child = value;
				_child.SizeChanged += OnChildSizeChanged;

				Children.Add(_child);
			}
		}

		public void UpdateBorderShape(IShape borderShape)
		{
			_borderShape = borderShape;

			UpdatePath();

			UpdateNativeBorder();
		}

		public void UpdateBackground(Paint? background)
		{
			if (_borderPath == null)
				return;

			_borderPath.Fill = background?.ToNative();
			_borderPath.Visibility = background != null ? UI.Xaml.Visibility.Visible : UI.Xaml.Visibility.Collapsed;
		}

		public void UpdateBorderBrush(Paint borderBrush)
		{
			if (_borderPath == null)
				return;

			_borderPath.Stroke = borderBrush.ToNative();

			UpdateNativeBorder();
		}

		public void UpdateBorderWidth(double borderWidth)
		{
			if (_borderPath == null)
				return;

			_borderPath.StrokeThickness = borderWidth;

			UpdateNativeBorder();
		}

		public void UpdateBorderDashArray(double[] borderDashArray)
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

		public void UpdateBorderDashOffset(double borderDashOffset)
		{
			if (_borderPath == null)
				return;

			_borderPath.StrokeDashOffset = borderDashOffset;
		}

		void UpdatePath()
		{
			if (Child == null || _borderShape == null)
				return;

			var width = Child.ActualWidth;
			var height = Child.ActualHeight;

			if (width <= 0 || height <= 0)
				return;

			var pathSize = new Graphics.Rectangle(0, 0, width, height);
			var shapePath = _borderShape.PathForBounds(pathSize);
			var geometry = shapePath.AsPathGeometry();

			if (_borderPath != null)
				_borderPath.Data = geometry;
		}

		void OnChildSizeChanged(object sender, SizeChangedEventArgs e)
		{
			UpdatePath();
		}

		void UpdateNativeBorder()
		{
			if (Child == null || _borderPath == null)
				return;

			bool hasBorder = _borderShape != null && _borderPath.Stroke != null && _borderPath.StrokeThickness > 0;

			// If we apply a custom border, remove the border from the NativeView
			if (Child is Control control)
			{
				control.BorderThickness =
					new UI.Xaml.Thickness(hasBorder ? 0 : 1);
			}

			if (Child is MauiTextBox textBox)
			{
				textBox.Style = hasBorder ?
					Application.Current.Resources["MauiBorderlessTextBoxStyle"] as Style :
					Application.Current.Resources["MauiTextBoxStyle"] as Style;
			}

			if (Child is CalendarDatePicker calendarDatePicker)
			{
				calendarDatePicker.Style = hasBorder ?
					Application.Current.Resources["MauiBorderlessCalendarDatePickerStyle"] as Style :
					Application.Current.Resources["MauiCalendarDatePickerStyle"] as Style;
			}
		}
	}
}