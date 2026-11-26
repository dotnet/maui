#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// A height at which a bottom sheet can rest.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Detents map to the native resting positions of the platform sheet: <c>UISheetPresentationController</c>
	/// detents on iOS and the collapsed, half-expanded and expanded states of the Material <c>BottomSheetBehavior</c>
	/// on Android. Because Android supports at most three resting positions, a sheet can declare at most three detents.
	/// </para>
	/// <para>
	/// Detents have value equality, so a detent created with <see cref="Fraction(double)"/> or <see cref="Height(double)"/>
	/// can be compared with, and selected through, an equal instance.
	/// </para>
	/// </remarks>
	[TypeConverter(typeof(BottomSheetDetentTypeConverter))]
	public sealed class BottomSheetDetent : IEquatable<BottomSheetDetent>
	{
		BottomSheetDetent(BottomSheetDetentKind kind, double value)
		{
			Kind = kind;
			Value = value;
		}

		/// <summary>
		/// Gets the platform's medium detent, about half of the available height.
		/// </summary>
		/// <remarks>On iOS the system deactivates the medium detent in compact height (iPhone landscape), where the sheet uses the large detent.</remarks>
		public static BottomSheetDetent Medium { get; } = new BottomSheetDetent(BottomSheetDetentKind.Medium, 0);

		/// <summary>
		/// Gets the platform's large detent, where the sheet is fully expanded.
		/// </summary>
		public static BottomSheetDetent Large { get; } = new BottomSheetDetent(BottomSheetDetentKind.Large, 0);

		/// <summary>
		/// Gets a detent whose height is the height the sheet's content needs, limited to the available height.
		/// </summary>
		/// <remarks>
		/// The content height is measured for a <see cref="ContentPage"/>, including its padding, and is measured again when the content changes.
		/// Other pages, such as a <see cref="NavigationPage"/>, use the available height.
		/// On iOS 15 this falls back to the large detent, because custom detents require iOS 16.
		/// </remarks>
		public static BottomSheetDetent FitContent { get; } = new BottomSheetDetent(BottomSheetDetentKind.FitContent, 0);

		/// <summary>
		/// Gets how this detent determines its height.
		/// </summary>
		public BottomSheetDetentKind Kind { get; }

		/// <summary>
		/// Gets the fraction of the available height for <see cref="BottomSheetDetentKind.Fraction"/> detents, or the height in
		/// device-independent units for <see cref="BottomSheetDetentKind.Height"/> detents. It is 0 for the other kinds.
		/// </summary>
		public double Value { get; }

		/// <summary>
		/// Creates a detent that is a fraction of the available height.
		/// </summary>
		/// <param name="fraction">The fraction of the available height. It is rounded to six decimals, and must then be greater than 0 and at most 1.</param>
		/// <returns>The detent.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="fraction"/> is not greater than 0 and at most 1.</exception>
		/// <remarks>On iOS 15 this falls back to the nearest of the medium and large detents, because custom detents require iOS 16.</remarks>
		public static BottomSheetDetent Fraction(double fraction)
		{
			// Rounded so that a percentage from XAML, such as "33.3%", equals the same fraction written in code.
			var rounded = Math.Round(fraction, 6);
			if (double.IsNaN(rounded) || rounded <= 0 || rounded > 1)
				throw new ArgumentOutOfRangeException(nameof(fraction), fraction, "The fraction must be greater than 0 and at most 1.");

			return new BottomSheetDetent(BottomSheetDetentKind.Fraction, rounded);
		}

		/// <summary>
		/// Creates a detent with a fixed height, limited to the available height.
		/// </summary>
		/// <param name="height">The height in device-independent units. It is rounded to three decimals, and must then be greater than 0 and finite.</param>
		/// <returns>The detent.</returns>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="height"/> is not greater than 0 or is not finite.</exception>
		/// <remarks>On iOS 15 this falls back to the nearest of the medium and large detents, because custom detents require iOS 16.</remarks>
		public static BottomSheetDetent Height(double height)
		{
			var rounded = Math.Round(height, 3);
			if (double.IsNaN(rounded) || double.IsInfinity(rounded) || rounded <= 0)
				throw new ArgumentOutOfRangeException(nameof(height), height, "The height must be greater than 0 and finite.");

			return new BottomSheetDetent(BottomSheetDetentKind.Height, rounded);
		}

		/// <inheritdoc/>
		public bool Equals(BottomSheetDetent? other) =>
			other is not null && Kind == other.Kind && Value.Equals(other.Value);

		/// <inheritdoc/>
		public override bool Equals(object? obj) => Equals(obj as BottomSheetDetent);

		/// <inheritdoc/>
		public override int GetHashCode() => ((int)Kind * 397) ^ Value.GetHashCode();

		/// <summary>Determines whether two detents are equal.</summary>
		/// <param name="left">The first detent.</param>
		/// <param name="right">The second detent.</param>
		/// <returns><see langword="true"/> if the detents are equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator ==(BottomSheetDetent? left, BottomSheetDetent? right) =>
			left is null ? right is null : left.Equals(right);

		/// <summary>Determines whether two detents are not equal.</summary>
		/// <param name="left">The first detent.</param>
		/// <param name="right">The second detent.</param>
		/// <returns><see langword="true"/> if the detents are not equal; otherwise, <see langword="false"/>.</returns>
		public static bool operator !=(BottomSheetDetent? left, BottomSheetDetent? right) => !(left == right);

		/// <summary>
		/// Returns the XAML form of the detent: <c>Medium</c>, <c>Large</c>, <c>FitContent</c>, a percentage such as <c>40%</c>,
		/// or a height such as <c>300</c>.
		/// </summary>
		/// <returns>The XAML form of the detent.</returns>
		public override string ToString() => Kind switch
		{
			BottomSheetDetentKind.Fraction => (Value * 100).ToString("0.###", CultureInfo.InvariantCulture) + "%",
			BottomSheetDetentKind.Height => Value.ToString(CultureInfo.InvariantCulture),
			_ => Kind.ToString(),
		};
	}
}
