namespace Xamarin.Forms.Shapes
{
	public class MatrixTransform : Transform
	{
		public static readonly BindableProperty MatrixProperty =
			BindableProperty.Create(nameof(Matrix), typeof(Matrix), typeof(MatrixTransform), new Matrix(),
				propertyChanged: OnTransformPropertyChanged);

		public Matrix Matrix
		{
			set { SetValue(MatrixProperty, value); }
			get { return (Matrix)GetValue(MatrixProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as MatrixTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			Value = Matrix;
		}
	}
}