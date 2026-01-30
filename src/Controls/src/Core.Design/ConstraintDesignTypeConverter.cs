using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for constraint values.
	/// </summary>
	public class ConstraintDesignTypeConverter : StringConverter
	{
		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH ConstraintTypeConverter.ConvertFrom
			var strValue = value?.ToString();
			return (strValue != null && double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _));
		}
	}
}
