using System;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Design
{
	public class GridLengthCollectionDesignTypeConverter : TypeConverter
	{
		// This tells XAML this converter can be used to process strings
		// Without this the values won't show up as hints
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (value?.ToString() is string strValue)
			{
				string[] lengths = strValue.Split(',');
				return lengths.All(GridLengthDesignTypeConverter.IsValid);
			}

			return false;
		}
	}
}
