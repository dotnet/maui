namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Transform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Transform']/Docs" />
	[System.ComponentModel.TypeConverter(typeof(TransformTypeConverter))]
	public class Transform : BindableObject
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Transform.xml" path="//Member[@MemberName='ValueProperty']/Docs" />
		public static readonly BindableProperty ValueProperty =
		   BindableProperty.Create(nameof(Value), typeof(Matrix), typeof(Transform), new Matrix());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Transform.xml" path="//Member[@MemberName='Value']/Docs" />
		public Matrix Value
		{
			set { SetValue(ValueProperty, value); }
			get { return (Matrix)GetValue(ValueProperty); }
		}
	}
}
