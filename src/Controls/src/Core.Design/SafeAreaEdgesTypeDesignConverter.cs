using System;
using System.ComponentModel;
using Controls.Core.Design;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls.Design
{
	public class SafeAreaEdgesTypeDesignConverter : StringConverter
	{
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH SafeAreaEdgesTypeConverter.ConvertFrom
			string strValue = value?.ToString()?.Trim();
			if (string.IsNullOrEmpty(strValue))
				return false;

			// Split by comma and check each part
			string[] parts = strValue.Split(',');
			
			// Must have 1, 2, or 4 parts
			if (parts.Length != 1 && parts.Length != 2 && parts.Length != 4)
				return false;

			// Each part must be a valid SafeAreaRegions value
			foreach (string part in parts)
			{
				string trimmedPart = part.Trim();
				if (!string.Equals(trimmedPart, "All", StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(trimmedPart, "None", StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(trimmedPart, "Default", StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(trimmedPart, "SoftInput", StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(trimmedPart, "Container", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			return true;
		}
	}
}