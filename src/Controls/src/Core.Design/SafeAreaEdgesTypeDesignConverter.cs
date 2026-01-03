using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Design
{
	public class SafeAreaEdgesTypeDesignConverter : StringConverter
	{
		private static readonly HashSet<string> ValidValues =
			new HashSet<string>(
				new[] { "All", "None", "Default", "SoftInput", "Container" },
				StringComparer.OrdinalIgnoreCase);

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH SafeAreaEdgesTypeConverter.ConvertFrom
			if (value?.ToString()?.Trim() is not string strValue || strValue.Length == 0)
				return false;

			// Split by comma and validate each part
			var parts = strValue.Split(',');

			// Must have 1, 2, or 4 parts
			if (parts.Length != 1 && parts.Length != 2 && parts.Length != 4)
				return false;

			// Each part must be a valid SafeAreaRegions value
			foreach (var part in parts)
			{
				if (!ValidValues.Contains(part.Trim()))
					return false;
			}

			return true;
		}
	}
}
