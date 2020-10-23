using System;
using System.Linq;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(ColumnDefinitionCollection))]
	public class ColumnDefinitionCollectionTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				var lengths = value.Split(',');
				var coldefs = new ColumnDefinitionCollection();
				var converter = new GridLengthTypeConverter();
				foreach (var length in lengths)
					coldefs.Add(new ColumnDefinition { Width = (GridLength)converter.ConvertFromInvariantString(length) });
				return coldefs;
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(ColumnDefinitionCollection)));
		}


		public override string ConvertToInvariantString(object value)
		{
			if (!(value is ColumnDefinitionCollection cdc))
				throw new NotSupportedException();
			var converter = new GridLengthTypeConverter();
			return string.Join(", ", cdc.Select(cd => converter.ConvertToInvariantString(cd.Width)));
		}
	}
}