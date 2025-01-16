#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Type converter for converting a properly formatted string to a Shadow.
	/// </summary>
	public class ShadowTypeConverter : TypeConverter
	{
		/// <summary>
		/// Checks whether the given <paramref name="sourceType" /> is a string.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="sourceType">The type to convert from.</param>
		/// <returns></returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <summary>
		/// Checks whether the given <paramref name="destinationType" /> is a string.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="destinationType">The type to convert to.</param>
		/// <returns></returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		/// <summary>
		/// Converts <paramref name="value" /> to a Shadow.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="culture">The culture to use for conversion.</param>
		/// <param name="value">The value to convert.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="value" /> is not a valid Shadow.</exception>
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue == null)
			{
				throw new ArgumentNullException(nameof(strValue));
			}

			var parts = strValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

			try
			{
				if (parts.Length == 3) // <color> | <float> | <float> e.g. #000000 4 4
				{
					var brush = new SolidColorBrush(Color.FromArgb(parts[0]));
					var offsetX = float.Parse(parts[1], CultureInfo.InvariantCulture);
					var offsetY = float.Parse(parts[2], CultureInfo.InvariantCulture);

					return new Shadow
					{
						Brush = brush,
						Offset = new Point(offsetX, offsetY)
					};
				}
				else if (parts.Length == 4) // <float> | <float> | <float> | <color> e.g. 4 4 16 #000000
				{
					var offsetX = float.Parse(parts[0], CultureInfo.InvariantCulture);
					var offsetY = float.Parse(parts[1], CultureInfo.InvariantCulture);
					var radius = float.Parse(parts[2], CultureInfo.InvariantCulture);
					var brush = new SolidColorBrush(Color.FromArgb(parts[3]));

					return new Shadow
					{
						Offset = new Point(offsetX, offsetY),
						Radius = radius,
						Brush = brush
					};
				}
				else if (parts.Length == 5) // <float> | <float> | <float> | <color> | <float> e.g. 4 4 16 #000000 0.5
				{
					var offsetX = float.Parse(parts[0], CultureInfo.InvariantCulture);
					var offsetY = float.Parse(parts[1], CultureInfo.InvariantCulture);
					var radius = float.Parse(parts[2], CultureInfo.InvariantCulture);
					var brush = new SolidColorBrush(Color.FromArgb(parts[3]));
					var opacity = float.Parse(parts[4], CultureInfo.InvariantCulture);

					return new Shadow
					{
						Offset = new Point(offsetX, offsetY),
						Radius = radius,
						Brush = brush,
						Opacity = opacity
					};
				}
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(Shadow)}.", ex);
			}

			throw new InvalidOperationException($"Cannot convert \"{strValue}\" into {typeof(IShadow)}.");
		}

		/// <summary>
		/// Converts a Shadow to a string.
		/// </summary>
		/// <param name="context">The context to use for conversion.</param>
		/// <param name="culture">The culture to use for conversion.</param>
		/// <param name="value">The Shadow to convert.</param>
		/// <param name="destinationType">The type to convert to.</param>
		/// <returns>A string representation of the Shadow.</returns>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="value" /> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="value" /> is not a Shadow or the Brush is not a SolidColorBrush.</exception>
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if(value is null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			if (value is Shadow shadow)
			{
				var offsetX = shadow.Offset.X.ToString(CultureInfo.InvariantCulture);
				var offsetY = shadow.Offset.Y.ToString(CultureInfo.InvariantCulture);
				var radius = shadow.Radius.ToString(CultureInfo.InvariantCulture);
				var color = (shadow.Brush as SolidColorBrush)?.Color.ToHex();
				var opacity = shadow.Opacity.ToString(CultureInfo.InvariantCulture);

				if (color == null)
				{
					throw new InvalidOperationException("Cannot convert Shadow to string: Brush is not a valid SolidColorBrush or has no Color.");
				}

				return $"{offsetX} {offsetY} {radius} {color} {opacity}";
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into string.");
		}
	}
}