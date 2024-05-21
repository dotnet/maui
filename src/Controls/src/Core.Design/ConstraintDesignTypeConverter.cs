using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Design
{
	public class ConstraintDesignTypeConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH ConstraintTypeConverter.ConvertFrom
			var strValue = value?.ToString();
			return (strValue != null && double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _));
		}
	}
}
