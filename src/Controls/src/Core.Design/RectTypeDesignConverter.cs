using System.ComponentModel;
using Controls.Core.Design;

namespace Microsoft.Maui.Controls.Design
{
	public class RectTypeDesignConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH Rect.TryParse
			int? count = DesignTypeConverterHelper.TryParseNumbers(value?.ToString(), ',', 4);
			return count == 4;
		}
	}
}
