#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A transform that uses a <see cref="Matrix"/> to perform arbitrary linear transformations.
	/// </summary>
	public class MatrixTransform : Transform
	{
		/// <summary>Bindable property for <see cref="Matrix"/>.</summary>
		public static readonly BindableProperty MatrixProperty =
			BindableProperty.Create(nameof(Matrix), typeof(Matrix), typeof(MatrixTransform), new Matrix(),
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>
		/// Gets or sets the transformation matrix. This is a bindable property.
		/// </summary>
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
