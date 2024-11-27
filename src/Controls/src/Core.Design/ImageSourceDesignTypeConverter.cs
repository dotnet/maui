using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Design
{
	public class ImageSourceDesignTypeConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH ImageSourceConverter.ConvertFrom. Note that MAUI runtime allows
			// empty or whitespace strings.
			if (value?.ToString() is string strValue)
				return Uri.TryCreate(strValue, UriKind.RelativeOrAbsolute, out Uri _);

			return false;
		}
	}
}
