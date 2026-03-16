#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A transform that scales an element horizontally and/or vertically from a specified center point.
	/// </summary>
	public class ScaleTransform : Transform
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ScaleTransform"/> class with default values.
		/// </summary>
		public ScaleTransform()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaleTransform"/> class with the specified scale factors.
		/// </summary>
		/// <param name="scaleX">The horizontal scale factor.</param>
		/// <param name="scaleY">The vertical scale factor.</param>
		public ScaleTransform(double scaleX, double scaleY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ScaleTransform"/> class with the specified scale factors and center point.
		/// </summary>
		/// <param name="scaleX">The horizontal scale factor.</param>
		/// <param name="scaleY">The vertical scale factor.</param>
		/// <param name="centerX">The x-coordinate of the scale center point.</param>
		/// <param name="centerY">The y-coordinate of the scale center point.</param>
		public ScaleTransform(double scaleX, double scaleY, double centerX, double centerY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
			CenterX = centerX;
			CenterY = centerY;
		}

		/// <summary>Bindable property for <see cref="ScaleX"/>.</summary>
		public static readonly BindableProperty ScaleXProperty =
			BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(ScaleTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="ScaleY"/>.</summary>
		public static readonly BindableProperty ScaleYProperty =
			BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(ScaleTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterX"/>.</summary>
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(ScaleTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterY"/>.</summary>
		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(ScaleTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>
		/// Gets or sets the horizontal scale factor. Default is 1.0. This is a bindable property.
		/// </summary>
		public double ScaleX
		{
			set { SetValue(ScaleXProperty, value); }
			get { return (double)GetValue(ScaleXProperty); }
		}

		/// <summary>
		/// Gets or sets the vertical scale factor. Default is 1.0. This is a bindable property.
		/// </summary>
		public double ScaleY
		{
			set { SetValue(ScaleYProperty, value); }
			get { return (double)GetValue(ScaleYProperty); }
		}

		/// <summary>
		/// Gets or sets the x-coordinate of the scale origin. Default is 0. This is a bindable property.
		/// </summary>
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the scale origin. Default is 0. This is a bindable property.
		/// </summary>
		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as ScaleTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			Value = new Matrix(ScaleX, 0, 0, ScaleY, CenterX * (1 - ScaleX), CenterY * (1 - ScaleY));
		}
	}
}