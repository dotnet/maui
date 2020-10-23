using System;
using System.Linq;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(RowDefinitionCollection))]
	public class RowDefinitionCollectionTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				var lengths = value.Split(',');
				var coldefs = new RowDefinitionCollection();
				var converter = new GridLengthTypeConverter();
				foreach (var length in lengths)
					coldefs.Add(new RowDefinition { Height = (GridLength)converter.ConvertFromInvariantString(length) });
				return coldefs;
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(RowDefinitionCollection)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is RowDefinitionCollection rdc))
				throw new NotSupportedException();
			var converter = new GridLengthTypeConverter();
			return string.Join(", ", rdc.Select(rd => converter.ConvertToInvariantString(rd.Height)));
		}
	}
}