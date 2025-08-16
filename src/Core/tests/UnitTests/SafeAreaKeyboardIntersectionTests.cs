using System;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	public class SafeAreaKeyboardIntersectionTests
	{
		[Theory]
		[InlineData(0, 0, 100, 100, 0, 70, 100, 100, 30)] // Partial overlap from bottom
		[InlineData(0, 0, 100, 100, 0, 0, 100, 200, 100)] // Full overlap
		[InlineData(0, 0, 100, 100, 0, 200, 100, 100, 0)] // No overlap (keyboard below)
		[InlineData(0, 50, 100, 100, 0, 0, 100, 30, 0)] // No overlap (keyboard above)
		[InlineData(0, 50, 100, 100, 0, 100, 100, 100, 50)] // Partial overlap at top
		public void CalculateRectangleIntersection_ReturnsCorrectHeight(
			double viewX, double viewY, double viewWidth, double viewHeight,
			double keyboardX, double keyboardY, double keyboardWidth, double keyboardHeight,
			double expectedIntersectionHeight)
		{
			// Arrange
			var viewRect = new TestRect(viewX, viewY, viewWidth, viewHeight);
			var keyboardRect = new TestRect(keyboardX, keyboardY, keyboardWidth, keyboardHeight);

			// Act
			var intersection = CalculateIntersection(viewRect, keyboardRect);
			var intersectionHeight = intersection.IsEmpty ? 0 : intersection.Height;

			// Assert
			Assert.Equal(expectedIntersectionHeight, intersectionHeight);
		}

		[Theory]
		[InlineData(0, 30, 30)] // Keyboard intersection smaller than safe area
		[InlineData(50, 30, 50)] // Keyboard intersection larger than safe area
		[InlineData(10, 10, 10)] // Equal values
		[InlineData(0, 0, 0)] // Both zero
		public void SafeAreaBottomInsetCalculation_PreservesLargerValue(
			double existingSafeAreaBottom, double keyboardIntersection, double expectedBottomInset)
		{
			// Act
			var result = Math.Max(existingSafeAreaBottom, keyboardIntersection);

			// Assert
			Assert.Equal(expectedBottomInset, result);
		}

		private static TestRect CalculateIntersection(TestRect rect1, TestRect rect2)
		{
			// Calculate intersection of two rectangles
			var left = Math.Max(rect1.X, rect2.X);
			var top = Math.Max(rect1.Y, rect2.Y);
			var right = Math.Min(rect1.X + rect1.Width, rect2.X + rect2.Width);
			var bottom = Math.Min(rect1.Y + rect1.Height, rect2.Y + rect2.Height);

			if (left >= right || top >= bottom)
			{
				return TestRect.Empty;
			}

			return new TestRect(left, top, right - left, bottom - top);
		}

		private struct TestRect
		{
			public double X { get; }
			public double Y { get; }
			public double Width { get; }
			public double Height { get; }

			public TestRect(double x, double y, double width, double height)
			{
				X = x;
				Y = y;
				Width = width;
				Height = height;
			}

			public bool IsEmpty => Width <= 0 || Height <= 0;

			public static TestRect Empty => new TestRect(0, 0, 0, 0);
		}
	}
}