using System;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(Binding))]
	[Obsolete("Obsolete since XF 5.0.0")] //not used anywhere
	public sealed class BindingTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value) => new Binding(value);

		public override string ConvertToInvariantString(object value) => throw new NotImplementedException();
	}
}