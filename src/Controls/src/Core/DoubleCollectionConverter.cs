using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DoubleCollectionConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.DoubleCollectionConverter']/Docs/*" />
	public class DoubleCollectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			string[] doubles = strValue.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			var doubleCollection = new DoubleCollection();

			foreach (string d in doubles)
			{
				if (double.TryParse(d, NumberStyles.Number, CultureInfo.InvariantCulture, out double number))
					doubleCollection.Add(number);
				else
					throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", d, typeof(double)));
			}

			return doubleCollection;
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not DoubleCollection dc)
				throw new NotSupportedException();
			return string.Join(", ", dc);
		}
	}
}
