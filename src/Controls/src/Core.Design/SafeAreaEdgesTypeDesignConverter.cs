using System;
using System.ComponentModel;
using Controls.Core.Design;
using Microsoft.Maui;

namespace Microsoft.Maui.Controls.Design
{
	public static class StringExtensions
	{
		public static bool IsEmpty(this string str) => string.IsNullOrEmpty(str);
		public static bool EqualsIgnoreCase(this string str, string other) =>
			str?.Equals(other, StringComparison.OrdinalIgnoreCase) ?? false;
	}

	public class SafeAreaEdgesTypeDesignConverter : StringConverter
	{
		private static readonly string[] ValidValues = { "All", "None", "Default", "SoftInput", "Container" };

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// MUST MATCH SafeAreaEdgesTypeConverter.ConvertFrom
			var strValue = value?.ToString()?.Trim();
			if (strValue.IsEmpty())
				return false;

			// Split by comma and validate each part
			var parts = strValue.Split(',');

			// Must have 1, 2, or 4 parts
			if (parts.Length != 1 && parts.Length != 2 && parts.Length != 4)
				return false;

			// Each part must be a valid SafeAreaRegions value
			foreach (var part in parts)
			{
				var trimmedPart = part.Trim();
				var isValidPart = false;

				foreach (var valid in ValidValues)
				{
					if (trimmedPart.EqualsIgnoreCase(valid))
					{
						isValidPart = true;
						break;
					}
				}

				if (!isValidPart)
					return false;
			}

			return true;
		}
	}
}
