namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.BindingTypeConverter")]
	[Xaml.TypeConversion(typeof(Binding))]
	public sealed class BindingTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return new Binding(value);
		}
	}
}