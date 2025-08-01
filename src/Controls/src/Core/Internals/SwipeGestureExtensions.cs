using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>
	/// Extension methods for swipe gesture-related operations
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static class SwipeGestureExtensions
	{
		/// <summary>
		/// Threshold for rotation values below which transformation is skipped (in degrees)
		/// </summary>
		const double RotationThreshold = 0.01;

		/// <summary>
		/// Normalizes a rotation angle to be within the range [0, 360)
		/// </summary>
		/// <param name="rotation">The rotation angle in degrees</param>
		/// <returns>The normalized rotation angle in the range [0, 360)</returns>
		internal static double NormalizeRotation(this double rotation)
		{
			return ((rotation % 360) + 360) % 360;
		}

		/// <summary>
		/// Validates that floating-point coordinates are finite (not NaN or Infinity)
		/// </summary>
		/// <param name="coordinates">Tuple containing x and y coordinates</param>
		/// <returns>True if both coordinates are finite, false otherwise</returns>
		internal static bool AreCoordinatesValid(this (float x, float y) coordinates)
		{
			return IsFiniteFloat(coordinates.x) && IsFiniteFloat(coordinates.y);
		}

		/// <summary>
		/// Validates that floating-point coordinates are finite (not NaN or Infinity)
		/// </summary>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>True if both coordinates are finite, false otherwise</returns>
		internal static bool AreCoordinatesValid(float x, float y)
		{
			return IsFiniteFloat(x) && IsFiniteFloat(y);
		}

		/// <summary>
		/// Validates that a rotation value is finite (not NaN or Infinity)
		/// </summary>
		/// <param name="rotation">The rotation angle</param>
		/// <returns>True if the rotation is finite, false otherwise</returns>
		internal static bool IsRotationValid(this double rotation)
		{
			return IsFiniteDouble(rotation);
		}

		/// <summary>
		/// Helper method to check if a float is finite (compatible with .NET Standard 2.0/2.1)
		/// </summary>
		static bool IsFiniteFloat(float value)
		{
			return !float.IsNaN(value) && !float.IsInfinity(value);
		}

		/// <summary>
		/// Helper method to check if a double is finite (compatible with .NET Standard 2.0/2.1)
		/// </summary>
		static bool IsFiniteDouble(double value)
		{
			return !double.IsNaN(value) && !double.IsInfinity(value);
		}

		internal static (float x, float y) TransformSwipeCoordinatesWithRotation(float x, float y, double rotation)
		{
			// Validate input coordinates for NaN or Infinity
			if (!AreCoordinatesValid(x, y))
			{
				return (0f, 0f);
			}

			// Validate rotation for NaN or Infinity
			if (!rotation.IsRotationValid())
			{
				return (x, y);
			}

			// Skip transformation for negligible rotation values to avoid unnecessary computation
			if (Math.Abs(rotation) < RotationThreshold)
			{
				return (x, y);
			}

			var normalizedRotation = rotation.NormalizeRotation();

			var radians = normalizedRotation * Math.PI / 180.0;
			var cos = Math.Cos(radians);
			var sin = Math.Sin(radians);

			var transformedX = (float)(x * cos - y * sin);
			var transformedY = (float)(x * sin + y * cos);

			// Validate transformed coordinates for NaN or Infinity
			if (!(transformedX, transformedY).AreCoordinatesValid())
			{
				return (x, y);
			}

			return (transformedX, transformedY);
		}

		internal static SwipeDirection TransformSwipeDirectionForRotation(SwipeDirection direction, double rotation)
		{
			// Validate rotation for NaN or Infinity
			if (!rotation.IsRotationValid())
			{
				return direction;
			}

			var normalizedRotation = rotation.NormalizeRotation();

			var rotationRounded = Math.Round(normalizedRotation / 90) * 90;
			
			if (Math.Abs(normalizedRotation - rotationRounded) > 45)
			{
				return direction;
			}
			
			var rotationSteps = (int)(rotationRounded / 90) % 4;
			
			return rotationSteps switch
			{
				0 => direction, // No rotation
				1 => direction switch // 90° clockwise
				{
					SwipeDirection.Up => SwipeDirection.Right,
					SwipeDirection.Right => SwipeDirection.Down,
					SwipeDirection.Down => SwipeDirection.Left,
					SwipeDirection.Left => SwipeDirection.Up,
					_ => direction
				},
				2 => direction switch // 180°
				{
					SwipeDirection.Up => SwipeDirection.Down,
					SwipeDirection.Right => SwipeDirection.Left,
					SwipeDirection.Down => SwipeDirection.Up,
					SwipeDirection.Left => SwipeDirection.Right,
					_ => direction
				},
				3 => direction switch // 270° clockwise (90° counter-clockwise)
				{
					SwipeDirection.Up => SwipeDirection.Left,
					SwipeDirection.Right => SwipeDirection.Up,
					SwipeDirection.Down => SwipeDirection.Right,
					SwipeDirection.Left => SwipeDirection.Down,
					_ => direction
				},
				_ => direction
			};
		}
	}
}
