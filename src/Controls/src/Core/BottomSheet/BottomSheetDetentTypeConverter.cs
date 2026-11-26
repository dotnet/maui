#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Converts a string to a <see cref="BottomSheetDetent"/>.
	/// </summary>
	/// <remarks>
	/// Accepted forms (case-insensitive): <c>Medium</c>, <c>Large</c>, <c>FitContent</c>, a percentage of the available height such as
	/// <c>40%</c>, or a height in device-independent units such as <c>300</c>.
	/// </remarks>
	public sealed class BottomSheetDetentTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <inheritdoc/>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue is null)
				return null;

			return Parse(strValue);
		}

		/// <inheritdoc/>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not BottomSheetDetent detent)
				throw new NotSupportedException();

			return detent.ToString();
		}

		internal static BottomSheetDetent Parse(string value)
		{
			var token = value.Trim();

			if (token.Equals(nameof(BottomSheetDetentKind.Medium), StringComparison.OrdinalIgnoreCase))
				return BottomSheetDetent.Medium;
			if (token.Equals(nameof(BottomSheetDetentKind.Large), StringComparison.OrdinalIgnoreCase))
				return BottomSheetDetent.Large;
			if (token.Equals(nameof(BottomSheetDetentKind.FitContent), StringComparison.OrdinalIgnoreCase))
				return BottomSheetDetent.FitContent;

			if (token.EndsWith("%", StringComparison.Ordinal))
			{
				var percentage = token.Substring(0, token.Length - 1).TrimEnd();
				if (double.TryParse(percentage, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent) && percent > 0 && percent <= 100)
					return BottomSheetDetent.Fraction(percent / 100);

				throw new FormatException($"Cannot convert \"{value}\" into {typeof(BottomSheetDetent)}: a percentage must be greater than 0% and at most 100%.");
			}

			if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var height))
			{
				// A bare value below 1 is almost certainly meant as a fraction, which is ambiguous with a height.
				if (!(height >= 1) || double.IsInfinity(height))
					throw new FormatException($"Cannot convert \"{value}\" into {typeof(BottomSheetDetent)}: a height must be at least 1. Use a percentage such as \"25%\" for a fraction of the available height.");

				return BottomSheetDetent.Height(height);
			}

			throw new FormatException($"Cannot convert \"{value}\" into {typeof(BottomSheetDetent)}. Use Medium, Large, FitContent, a percentage such as \"40%\", or a height such as \"300\".");
		}
	}

	/// <summary>
	/// Converts a comma-separated string to a list of <see cref="BottomSheetDetent"/> values, for example <c>"Medium, Large"</c>
	/// or <c>"30%, FitContent"</c>.
	/// </summary>
	/// <remarks>Each entry accepts the forms described for <see cref="BottomSheetDetentTypeConverter"/>.</remarks>
	public sealed class BottomSheetDetentCollectionTypeConverter : TypeConverter
	{
		/// <inheritdoc/>
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		/// <inheritdoc/>
		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		/// <inheritdoc/>
		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();
			if (strValue is null)
				return null;

			var parts = strValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
			var detents = new List<BottomSheetDetent>(parts.Length);
			foreach (var part in parts)
			{
				if (string.IsNullOrWhiteSpace(part))
					continue;

				detents.Add(BottomSheetDetentTypeConverter.Parse(part));
			}

			if (detents.Count == 0)
				throw new FormatException($"Cannot convert \"{strValue}\" into a list of {typeof(BottomSheetDetent)}: at least one detent is required.");

			return detents.AsReadOnly();
		}

		/// <inheritdoc/>
		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not IEnumerable<BottomSheetDetent> detents)
				throw new NotSupportedException();

			return string.Join(", ", detents);
		}
	}
}
