using System;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Design
{
	public class VisibilityDesignTypeConverter : StringConverter
	{
		private static readonly string[] validValues = new[] { "Collapse", "Hidden", bool.FalseString, bool.TrueString, "Visible" };
		private static readonly StandardValuesCollection standardValues = new StandardValuesCollection(validValues);

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
		override public bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
		override public StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) => standardValues;
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH VisibilityConverter.ConvertFrom
			if (value?.ToString()?.Trim() is string strValue)
				return validValues.Contains(strValue, StringComparer.OrdinalIgnoreCase);

			return false;
		}
	}
}
