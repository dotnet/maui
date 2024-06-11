namespace Microsoft.Maui.TestCases.Tests
{
	internal abstract class TypeConverter
	{
		public abstract bool CanConvertTo(object source, Type targetType);

		public abstract object ConvertTo(object source, Type targetType);
	}
}