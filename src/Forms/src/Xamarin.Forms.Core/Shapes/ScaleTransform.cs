namespace Xamarin.Forms.Shapes
{
	public class ScaleTransform : Transform
	{
		public ScaleTransform()
		{

		}

		public ScaleTransform(double scaleX, double scaleY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
		}

		public ScaleTransform(double scaleX, double scaleY, double centerX, double centerY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
			CenterX = centerX;
			CenterY = centerY;
		}

		public static readonly BindableProperty ScaleXProperty =
			BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(ScaleTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty ScaleYProperty =
			BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(ScaleTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(ScaleTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(ScaleTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public double ScaleX
		{
			set { SetValue(ScaleXProperty, value); }
			get { return (double)GetValue(ScaleXProperty); }
		}

		public double ScaleY
		{
			set { SetValue(ScaleYProperty, value); }
			get { return (double)GetValue(ScaleYProperty); }
		}

		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

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