using System;
using System.ComponentModel;
using System.Linq;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for font size values.
	/// </summary>
	public class FontSizeDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FontSizeDesignTypeConverter"/> class.
		/// </summary>
		public FontSizeDesignTypeConverter()
		{ }

		/// <inheritdoc/>
		protected override string[] KnownValues
			=> new[] { "Default", "Micro", "Small", "Medium", "Large", "Body", "Header", "Title", "Subtitle", "Caption" };

		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (KnownValues.Any(v => value?.ToString()?.Equals(v, StringComparison.Ordinal) ?? false))
				return true;

			if (double.TryParse(value?.ToString(), out var d))
				return true;

			return false;
		}
	}
}