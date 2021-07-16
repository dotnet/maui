using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public class DoubleCollectionConverter : StringTypeConverterBase
	{
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