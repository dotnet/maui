#nullable enable
using System;

namespace Microsoft.Maui.Layouts
{
	/// <summary>
	/// Represents a value that tracks both density-independent (dp) and physical pixel values
	/// to enable precise pixel-aware layout calculations.
	/// </summary>
	internal readonly struct DensityValue : IEquatable<DensityValue>
	{
		private const double Epsilon = 0.00001;

		/// <summary>
		/// Gets the value in density-independent pixels (dp).
		/// </summary>
		public double Dp { get; }

		/// <summary>
		/// Gets the display density factor.
		/// </summary>
		public double Density { get; }

		/// <summary>
		/// Gets the raw pixel value (dp * density) without rounding.
		/// </summary>
		public double RawPx => Dp * Density;

		/// <summary>
		/// Gets the rounded pixel value suitable for display.
		/// </summary>
		public int RoundedPx => (int)Math.Round(RawPx);

		/// <summary>
		/// Initializes a new instance of the DensityValue struct.
		/// </summary>
		/// <param name="dp">The value in density-independent pixels.</param>
		/// <param name="density">The display density factor.</param>
		public DensityValue(double dp, double density)
		{
			Dp = dp;
			Density = density;
		}

		/// <summary>
		/// Creates a DensityValue from a pixel value and density.
		/// </summary>
		/// <param name="pixels">The pixel value.</param>
		/// <param name="density">The display density factor.</param>
		/// <returns>A DensityValue representing the equivalent dp value.</returns>
		public static DensityValue FromPixels(double pixels, double density)
		{
			return new DensityValue(pixels / density, density);
		}

		/// <summary>
		/// Adds two DensityValue instances.
		/// </summary>
		public static DensityValue operator +(DensityValue left, DensityValue right)
		{
			if (Math.Abs(left.Density - right.Density) > Epsilon)
			{
				throw new ArgumentException("Cannot add DensityValues with different densities.");
			}

			return new DensityValue(left.Dp + right.Dp, left.Density);
		}

		/// <summary>
		/// Subtracts two DensityValue instances.
		/// </summary>
		public static DensityValue operator -(DensityValue left, DensityValue right)
		{
			if (Math.Abs(left.Density - right.Density) > Epsilon)
			{
				throw new ArgumentException("Cannot subtract DensityValues with different densities.");
			}

			return new DensityValue(left.Dp - right.Dp, left.Density);
		}

		/// <summary>
		/// Multiplies a DensityValue by a scalar.
		/// </summary>
		public static DensityValue operator *(DensityValue value, double scalar)
		{
			return new DensityValue(value.Dp * scalar, value.Density);
		}

		/// <summary>
		/// Multiplies a DensityValue by a scalar.
		/// </summary>
		public static DensityValue operator *(double scalar, DensityValue value)
		{
			return value * scalar;
		}

		/// <summary>
		/// Divides a DensityValue by a scalar.
		/// </summary>
		public static DensityValue operator /(DensityValue value, double scalar)
		{
			return new DensityValue(value.Dp / scalar, value.Density);
		}

		/// <summary>
		/// Implicitly converts a DensityValue to its dp value.
		/// </summary>
		public static implicit operator double(DensityValue value)
		{
			return value.Dp;
		}

		/// <summary>
		/// Distributes a total pixel amount across multiple DensityValue instances,
		/// accumulating rounding errors and applying them to the final elements.
		/// This implements Android's approach of assigning remainder pixels to the last element.
		/// </summary>
		/// <param name="totalPixels">The total pixels to distribute.</param>
		/// <param name="density">The display density.</param>
		/// <param name="portions">The relative portions for each element.</param>
		/// <returns>An array of pixel values that sum exactly to totalPixels.</returns>
		public static int[] DistributePixels(double totalPixels, double density, double[] portions)
		{
			if (portions.Length == 0)
				return Array.Empty<int>();

			var totalPortions = 0.0;
			foreach (var portion in portions)
			{
				totalPortions += portion;
			}

			if (totalPortions <= 0)
				return new int[portions.Length]; // All zeros

			var result = new int[portions.Length];
			var targetTotal = (int)Math.Round(totalPixels);
			var assignedTotal = 0;

			// Calculate ideal pixels per portion
			var idealPixelsPerUnit = totalPixels / totalPortions;

			// Assign pixels to all but the last element using floor
			for (int i = 0; i < portions.Length - 1; i++)
			{
				var idealPixels = idealPixelsPerUnit * portions[i];
				result[i] = (int)Math.Floor(idealPixels);
				assignedTotal += result[i];
			}

			// Assign remaining pixels to the last element (this accumulates rounding error)
			if (portions.Length > 0)
			{
				result[portions.Length - 1] = targetTotal - assignedTotal;
			}

			return result;
		}

		public bool Equals(DensityValue other)
		{
			return Math.Abs(Dp - other.Dp) < Epsilon &&
				   Math.Abs(Density - other.Density) < Epsilon;
		}

		public override bool Equals(object? obj)
		{
			return obj is DensityValue other && Equals(other);
		}

		public override int GetHashCode()
		{
			return (Dp, Density).GetHashCode();
		}

		public static bool operator ==(DensityValue left, DensityValue right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(DensityValue left, DensityValue right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return $"{Dp:F2}dp ({RawPx:F2}px @ {Density:F2}x)";
		}
	}
}