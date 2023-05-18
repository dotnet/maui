#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/MatrixTransform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.MatrixTransform']/Docs/*" />
	public class MatrixTransform : Transform
	{
		/// <summary>Bindable property for <see cref="Matrix"/>.</summary>
		public static readonly BindableProperty MatrixProperty =
			BindableProperty.Create(nameof(Matrix), typeof(Matrix), typeof(MatrixTransform), new Matrix(),
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/MatrixTransform.xml" path="//Member[@MemberName='Matrix']/Docs/*" />
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
