using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Design
{
	public class VisibilityDesignTypeConverter : StringConverter
	{
		private static readonly string[] validValues = ["Collapse", "Hidden", bool.FalseString, bool.TrueString, "Visible"];
		private static readonly HashSet<string> supportedValues = new HashSet<string>(validValues, StringComparer.OrdinalIgnoreCase);
		private static readonly StandardValuesCollection standardValues = new StandardValuesCollection(validValues);

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) => true;
		override public bool GetStandardValuesSupported(ITypeDescriptorContext context) => true;
		override public StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) => standardValues;
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH VisibilityConverter.ConvertFrom
			if (value?.ToString()?.Trim() is string strValue)
				return supportedValues.Contains(strValue);

			return false;
		}
	}
}
