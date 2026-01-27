using System;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for GridLength collection values.
	/// </summary>
	public class GridLengthCollectionDesignTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

		/// <inheritdoc/>
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
