using System.ComponentModel;
using Controls.Core.Design;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for Point values.
	/// </summary>
	public class PointTypeDesignConverter : StringConverter
	{
		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH Point.TryParse
			int? count = DesignTypeConverterHelper.TryParseNumbers(value?.ToString(), ',', 2);
			return count == 2;
		}
	}
}
