using System;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>
	/// Extension methods for swipe gesture-related operations
	/// </summary>
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
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <returns>True if both coordinates are finite, false otherwise</returns>
		internal static bool AreCoordinatesValid(double x, double y)
		{
			return !double.IsNaN(x) && !double.IsInfinity(x) &&
				   !double.IsNaN(y) && !double.IsInfinity(y);
		}

		internal static (double x, double y) TransformSwipeCoordinatesWithRotation(double x, double y, double rotation)
		{
			// Skip transformation for negligible rotation values to avoid unnecessary computation
			if (Math.Abs(rotation) < RotationThreshold)
			{
				return (x, y);
			}

			// Normalize rotation to [0, 360) range to handle negative angles and values > 360
			var normalizedRotation = rotation.NormalizeRotation();
			var radians = normalizedRotation * Math.PI / 180.0;

			var cos = Math.Cos(radians);
			var sin = Math.Sin(radians);
			var transformedX = x * cos - y * sin;
			var transformedY = x * sin + y * cos;

			// Validate transformed coordinates for NaN or Infinity
			if (!AreCoordinatesValid(transformedX, transformedY))
			{
				return (x, y);
			}

			return (transformedX, transformedY);
		}

		internal static SwipeDirection TransformSwipeDirectionForRotation(SwipeDirection direction, double rotation)
		{
			// Normalize rotation to [0, 360) range to handle negative angles and values > 360
			var normalizedRotation = rotation.NormalizeRotation();

			// Round to nearest 90-degree increment (0, 90, 180, 270) to work with cardinal rotations
			var rotationRounded = Math.Round(normalizedRotation / 90) * 90;

			// Only transform direction if rotation is close to a cardinal angle (within 45 degrees) to prevent transformation for arbitrary rotations
			if (Math.Abs(normalizedRotation - rotationRounded) > 45)
			{
				return direction;
			}

			// Calculate rotation steps as multiples of 90 degrees (0=0°, 1=90°, 2=180°, 3=270°) for directional transformation
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
