#nullable enable
using System;

namespace Microsoft.Maui
{
	public readonly struct Font : IEquatable<Font>
	{
		public string? Family { get; }

		public double Size { get; }

		public FontSlant Slant { get; }

		public bool IsDefault => Family == null && Size == 0 && Slant == FontSlant.Default && Weight == FontWeight.Regular;

		static Font _default = default(Font).WithWeight(FontWeight.Regular);
		public static Font Default => _default;

		readonly FontWeight _weight;

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
		public bool AutoScalingEnabled
		{
			get => !_disableScaling;
		}

		public Font WithAutoScaling(bool enabled)
		{
			return new Font(Family, Size, Slant, Weight, enabled);
		}

		public Font WithSize(double size)
		{
			return new Font(Family, size, Slant, Weight, AutoScalingEnabled);
		}

		public Font WithSlant(FontSlant fontSlant)
		{
			return new Font(Family, Size, fontSlant, Weight, AutoScalingEnabled);
		}

		public Font WithWeight(FontWeight weight)
		{
			return new Font(Family, Size, Slant, weight, AutoScalingEnabled);
		}

		public Font WithWeight(FontWeight weight, FontSlant fontSlant)
		{
			return new Font(Family, Size, fontSlant, weight, AutoScalingEnabled);
		}

		public static Font OfSize(string? name, double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default, bool enableScaling = true) =>
			new(name, size, fontSlant, weight, enableScaling);

		public static Font SystemFontOfSize(double size, FontWeight weight = FontWeight.Regular, FontSlant fontSlant = FontSlant.Default, bool enableScaling = true) =>
			new(null, size, fontSlant, weight, enableScaling);

		public static Font SystemFontOfWeight(FontWeight weight, FontSlant fontSlant = FontSlant.Default, bool enableScaling = true) =>
			new(null, default(double), fontSlant, weight, enableScaling);

		bool Equals(Font other)
		{
			return string.Equals(Family, other.Family)
				&& Size.Equals(other.Size)
				&& Weight == other.Weight
				&& Slant == other.Slant
				&& AutoScalingEnabled == other.AutoScalingEnabled;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Font)obj);
		}

		public override int GetHashCode() => (Family, Size, Weight, Slant, AutoScalingEnabled).GetHashCode();

		public static bool operator ==(Font left, Font right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Font left, Font right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
			=> $"Family: {Family}, Size: {Size}, Weight: {Weight}, Slant: {Slant}, AutoScalingEnabled: {AutoScalingEnabled}";

		bool IEquatable<Font>.Equals(Font other)
		{
			return Equals(other);
		}
	}
}