#nullable enable
using System;
using System.ComponentModel;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a font, including family, size, weight, slant, and auto-scaling settings.
	/// </summary>
	/// <remarks>
	/// The font used to display text. Supported fonts and their rendering depend on the platform.
	/// </remarks>
	public readonly struct Font : IEquatable<Font>
	{
		/// <summary>
		/// Gets the font family name, or null for the default system font.
		/// </summary>
		public string? Family { get; }

		/// <summary>
		/// Gets the size of the font.
		/// </summary>
		public double Size { get; }

		/// <summary>
		/// Gets the slant (style) of the font (e.g., default or italic).
		/// </summary>
		public FontSlant Slant { get; }

		/// <summary>
		/// Gets a value indicating whether this font is the default font (no family, non-positive size, default slant, and regular weight).
		/// </summary>
		public bool IsDefault => Family is null && (Size <= 0 || double.IsNaN(Size)) && Slant == FontSlant.Default && Weight == FontWeight.Regular;

		static Font _default = default(Font).WithWeight(FontWeight.Regular);
		/// <summary>
		/// Gets the default font for the platform.
		/// </summary>
		public static Font Default => _default;

		readonly FontWeight _weight;

		/// <summary>
		/// Gets the weight of the font, defaulting to <see cref="FontWeight.Regular"/> if unspecified.
		/// </summary>
		/// <value>The font weight (e.g., Bold, Regular).</value>
		public FontWeight Weight
		{
			get => _weight <= 0 ? FontWeight.Regular : _weight;
		}

		private Font(string? family, double size, FontSlant slant, FontWeight weight, bool enableScaling) : this()
		{
			Family = family;
			Size = size;
			Slant = slant;
			_weight = weight;
			_disableScaling = !enableScaling;
		}


		readonly bool _disableScaling;
		/// <summary>
		/// Gets a value indicating whether auto-scaling is enabled for this font.
		/// </summary>
		public bool AutoScalingEnabled
		{
			get => !_disableScaling;
		}

		/// <summary>
		/// Returns a new <see cref="Font"/> with auto-scaling enabled or disabled.
		/// </summary>
		/// <param name="enabled">If true, auto-scaling is enabled.</param>
		/// <returns>A new font instance with the specified auto-scaling setting.</returns>
		public Font WithAutoScaling(bool enabled)
		{
			return new Font(Family, Size, Slant, Weight, enabled);
		}

		/// <summary>
		/// Returns a new <see cref="Font"/> with the specified size.
		/// </summary>
		/// <param name="size">The desired font size.</param>
		/// <returns>A new font instance with the specified size.</returns>
		public Font WithSize(double size)
		{
			return new Font(Family, size, Slant, Weight, AutoScalingEnabled);
		}

		/// <summary>
		/// Returns a new <see cref="Font"/> with the specified slant (style).
		/// </summary>
		/// <param name="fontSlant">The slant (e.g., italic) to apply.</param>
		/// <returns>A new font instance with the specified slant.</returns>
		public Font WithSlant(FontSlant fontSlant)
		{
			return new Font(Family, Size, fontSlant, Weight, AutoScalingEnabled);
		}

		/// <summary>
		/// Returns a new <see cref="Font"/> with the specified weight.
		/// </summary>
		/// <param name="weight">The weight to apply (e.g., Bold, Regular).</param>
		/// <returns>A new font instance with the specified weight.</returns>
		public Font WithWeight(FontWeight weight)
		{
			return new Font(Family, Size, Slant, weight, AutoScalingEnabled);
		}

		/// <summary>
		/// Returns a new <see cref="Font"/> with the specified weight and slant.
		/// </summary>
		/// <param name="weight">The weight to apply.</param>
		/// <param name="fontSlant">The slant to apply.</param>
		/// <returns>A new font instance with the specified weight and slant.</returns>
		public Font WithWeight(FontWeight weight, FontSlant fontSlant)
		{
			return new Font(Family, Size, fontSlant, weight, AutoScalingEnabled);
		}

		/// <summary>
		/// Creates a font with the specified family, size, weight, slant, and auto-scaling.
		/// </summary>
		/// <param name="name">The font family name or system font alias.</param>
		/// <param name="size">The desired font size.</param>
		/// <param name="weight">The font weight.</param>
		/// <param name="fontSlant">The font slant.</param>
		/// <param name="enableScaling">Whether auto-scaling is enabled.</param>
		/// <returns>A <see cref="Font"/> instance with the specified settings.</returns>
		public static Font OfSize(string? name, double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default, bool enableScaling = true)
		{
			return new(name, size, fontSlant, weight, enableScaling);
		}

		/// <summary>
		/// Returns a system font of the specified size, weight, slant, and auto-scaling.
		/// </summary>
		/// <param name="size">The desired font size.</param>
		/// <param name="weight">The font weight.</param>
		/// <param name="fontSlant">The font slant.</param>
		/// <param name="enableScaling">Whether auto-scaling is enabled.</param>
		/// <returns>A <see cref="Font"/> instance of the system font.</returns>
		public static Font SystemFontOfSize(double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default, bool enableScaling = true) =>
			new(null, size, fontSlant, weight, enableScaling);

		/// <summary>
		/// Returns a font instance with the specified weight, slant, and auto-scaling with a default size.
		/// </summary>
		/// <param name="weight">The font weight.</param>
		/// <param name="fontSlant">The font slant.</param>
		/// <param name="enableScaling">Whether auto-scaling is enabled.</param>
		/// <returns>A <see cref="Font"/> instance.</returns>
		public static Font SystemFontOfWeight(FontWeight weight, FontSlant fontSlant = FontSlant.Default, bool enableScaling = true) =>
			new(null, default(double), fontSlant, weight, enableScaling);

		bool Equals(Font other)
		{
			return string.Equals(Family, other.Family, StringComparison.Ordinal)
				&& Size.Equals(other.Size)
				&& Weight == other.Weight
				&& Slant == other.Slant
				&& AutoScalingEnabled == other.AutoScalingEnabled;
		}

		/// <summary>
		/// Determines whether the specified object is equal to this font.
		/// </summary>
		/// <param name="obj">The object to compare.</param>
		/// <returns>True if equal; otherwise, false.</returns>
		public override bool Equals(object? obj)
		{
			if (obj is null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Font)obj);
		}

		/// <summary>
		/// Returns the hash code for this font.
		/// </summary>
		/// <returns>A 32-bit signed hash code.</returns>
		public override int GetHashCode() => (Family, Size, Weight, Slant, AutoScalingEnabled).GetHashCode();

		/// <summary>
		/// Determines whether two fonts are equal.
		/// </summary>
		/// <param name="left">The first font to compare.</param>
		/// <param name="right">The second font to compare.</param>
		/// <returns>True if equal; otherwise, false.</returns>
		public static bool operator ==(Font left, Font right) => left.Equals(right);

		/// <summary>
		/// Determines whether two fonts are not equal.
		/// </summary>
		/// <param name="left">The first font to compare.</param>
		/// <param name="right">The second font to compare.</param>
		/// <returns>True if not equal; otherwise, false.</returns>
		public static bool operator !=(Font left, Font right) => !left.Equals(right);

		/// <summary>
		/// Returns a string that represents this font.
		/// </summary>
		/// <returns>A string describing the font's properties.</returns>
		public override string ToString()
			=> $"Family: {Family}, Size: {Size}, Weight: {Weight}, Slant: {Slant}, AutoScalingEnabled: {AutoScalingEnabled}";

		bool IEquatable<Font>.Equals(Font other)
		{
			return Equals(other);
		}
	}
}
