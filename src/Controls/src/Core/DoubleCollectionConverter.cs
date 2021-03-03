using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public class DoubleCollectionConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			string[] doubles = value.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
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

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is DoubleCollection dc))
				throw new NotSupportedException();
			return string.Join(", ", dc);
		}
	}
}