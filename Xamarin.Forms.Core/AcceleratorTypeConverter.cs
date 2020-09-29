namespace Xamarin.Forms
{
	public class AcceleratorTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
				return null;

			return Accelerator.FromString(value);
		}
	}
}