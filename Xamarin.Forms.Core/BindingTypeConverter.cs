namespace Xamarin.Forms
{
	public sealed class BindingTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return new Binding(value);
		}
	}
}