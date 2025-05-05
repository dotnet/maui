using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DoubleCollectionConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.DoubleCollectionConverter']/Docs/*" />
	public class DoubleCollectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string)
				|| sourceType == typeof(double[])
				|| sourceType == typeof(float[]);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			if (value is double[] doublesArray)
			{
				return (DoubleCollection)doublesArray;
			}
			else if (value is float[] floatsArray)
			{
				return (DoubleCollection)floatsArray;
			}

			var strValue = value.ToString();
			if (strValue is null)
			{
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(DoubleCollection)));
			}

			string[] doubles = strValue.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
			var doubleCollection = new DoubleCollection();

			foreach (string d in doubles)
			{
				if (double.TryParse(d, NumberStyles.Number, CultureInfo.InvariantCulture, out double number))
				{
					doubleCollection.Add(number);
				}
				else
				{
					throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", d, typeof(double)));
				}
			}

			return doubleCollection;
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not DoubleCollection dc)
			{
				throw new NotSupportedException();
			}

			return string.Join(", ", dc);
		}
	}
}
