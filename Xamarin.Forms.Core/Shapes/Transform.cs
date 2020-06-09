namespace Xamarin.Forms.Shapes
{
    [TypeConverter(typeof(TransformTypeConverter))]
    public class Transform : BindableObject
    {
        internal static readonly BindableProperty ValueProperty =
           BindableProperty.Create(nameof(Value), typeof(Matrix), typeof(Transform), new Matrix());

        public Matrix Value
        {
            set { SetValue(ValueProperty, value); }
            get { return (Matrix)GetValue(ValueProperty); }
        }
    }
}