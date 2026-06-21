#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Base class for all transforms that can be applied to shapes.
	/// </summary>
	[System.ComponentModel.TypeConverter(typeof(TransformTypeConverter))]
	public class Transform : BindableObject
	{
		/// <summary>Bindable property for <see cref="Value"/>.</summary>
		public static readonly BindableProperty ValueProperty =
		   BindableProperty.Create(nameof(Value), typeof(Matrix), typeof(Transform), new Matrix());

		/// <summary>
		/// Gets or sets the transformation matrix. This is a bindable property.
		/// </summary>
		public Matrix Value
		{
			set { SetValue(ValueProperty, value); }
			get { return (Matrix)GetValue(ValueProperty); }
		}
	}
}
