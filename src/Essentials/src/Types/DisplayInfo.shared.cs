using System;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Devices
{
	/// <summary>
	/// Represents information about the device's screen.
	/// </summary>
	public readonly struct DisplayInfo : IEquatable<DisplayInfo>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayInfo"/> class.
		/// </summary>
		/// <param name="width">The width of the display.</param>
		/// <param name="height">The height of the display.</param>
		/// <param name="density">The screen density.</param>
		/// <param name="orientation">The current orientation.</param>
		/// <param name="rotation">The rotation of the device.</param>
		public DisplayInfo(double width, double height, double density, DisplayOrientation orientation, DisplayRotation rotation)
		{
			Width = width;
			Height = height;
			Density = density;
			Orientation = orientation;
			Rotation = rotation;
			RefreshRate = 0;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DisplayInfo"/> class.
		/// </summary>
		/// <param name="width">The width of the display.</param>
		/// <param name="height">The height of the display.</param>
		/// <param name="density">The screen density.</param>
		/// <param name="orientation">The current orientation.</param>
		/// <param name="rotation">The rotation of the device.</param>
		/// <param name="rate">The refresh rate of the display.</param>
		public DisplayInfo(double width, double height, double density, DisplayOrientation orientation, DisplayRotation rotation, float rate)
		{
			Width = width;
			Height = height;
			Density = density;
			Orientation = orientation;
			Rotation = rotation;
			RefreshRate = rate;
		}

		/// <summary>
		/// Gets the width of the screen (in pixels) for the current <see cref="Orientation"/>.
		/// </summary>
		public double Width { get; }

		/// <summary>
		/// Gets the height of the screen (in pixels) for the current <see cref="Orientation"/>.
		/// </summary>
		public double Height { get; }

		/// <summary>
		/// Gets a value representing the screen density.
		/// </summary>
		/// <remarks>
		/// <para>The density is the scaling or a factor that can be used to convert between physical pixels and scaled pixels. For example, on high resolution displays, the physical number of pixels increases, but the scaled pixels remain the same.</para>
		/// <para>In a practical example for iOS, the Retina display will have a density of 2.0 or 3.0, but the units used to lay out a view does not change much. A view with a UI width of 100 may be 100 physical pixels (density = 1) on a non-Retina device, but be 200 physical pixels (density = 2) on a Retina device.</para>
		/// <para>On Windows, the density works similarly, and may often relate to the scale used in the display. On some monitors, the scale is set to 100% (density = 1), but on other high resolution monitors, the scale may be set to 200% (density = 2) or even 250% (density = 2.5).</para>
		/// </remarks>
		public double Density { get; }

		/// <summary>
		/// Gets the orientation of the device's display.
		/// </summary>
		public DisplayOrientation Orientation { get; }

		/// <summary>
		/// Gets the orientation of the device's display.
		/// </summary>
		public DisplayRotation Rotation { get; }

		/// <summary>
		/// Gets the refresh rate (in Hertz) of the device's display.
		/// </summary>
		public float RefreshRate { get; }

		/// <summary>
		/// Equality operator for equals.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are equal, otherwise <see langword="false"/>.</returns>
		public static bool operator ==(DisplayInfo left, DisplayInfo right) =>
			left.Equals(right);

		/// <summary>
		/// Inequality operator.
		/// </summary>
		/// <param name="left">Left to compare.</param>
		/// <param name="right">Right to compare.</param>
		/// <returns><see langword="true"/> if objects are not equal, otherwise <see langword="false"/>.</returns>
		public static bool operator !=(DisplayInfo left, DisplayInfo right) =>
			!left.Equals(right);

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		public override bool Equals(object obj) =>
			(obj is DisplayInfo metrics) && Equals(metrics);

		/// <summary>
		/// Compares the underlying <see cref="DisplayInfo"/> instances.
		/// </summary>
		/// <param name="other"><see cref="DisplayInfo"/> object to compare with.</param>
		/// <returns><see langword="true"/> if they are equal, otherwise <see langword="false"/>.</returns>
		/// <remarks>Equality is established by comparing if <see cref="Height"/>, <see cref="Width"/>, <see cref="Density"/>, <see cref="Orientation"/> and <see cref="Rotation"/> are all equal.</remarks>
		public bool Equals(DisplayInfo other) =>
			Width.Equals(other.Width) &&
			Height.Equals(other.Height) &&
			Density.Equals(other.Density) &&
			Orientation.Equals(other.Orientation) &&
			Rotation.Equals(other.Rotation) &&
			RefreshRate.Equals(other.RefreshRate);

		/// <summary>
		/// Gets the hash code for this display info instance.
		/// </summary>
		/// <returns>The computed hash code for this device idiom or <c>0</c> when the device platform is <see langword="null"/>.</returns>
		/// <remarks>The hash code is computed from <see cref="Height"/>, <see cref="Width"/>, <see cref="Density"/>, <see cref="Orientation"/> and <see cref="Rotation"/>.</remarks>
		public override int GetHashCode() =>
			(Height, Width, Density, Orientation, Rotation).GetHashCode();

		/// <summary>
		/// Returns a string representation of the current values of this display info instance.
		/// </summary>
		/// <returns>A string representation of this instance in the format of <c>Height: {value}, Width: {value}, Density: {value}, Orientation: {value}, Rotation: {value}</c>.</returns>
		public override string ToString() =>
			$"{nameof(Height)}: {Height}, {nameof(Width)}: {Width}, " +
			$"{nameof(Density)}: {Density}, {nameof(Orientation)}: {Orientation}, " +
			$"{nameof(Rotation)}: {Rotation}";
	}
}
