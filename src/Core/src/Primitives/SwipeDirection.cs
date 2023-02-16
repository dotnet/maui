using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <include file="../../docs/Microsoft.Maui/SwipeDirection.xml" path="Type[@FullName='Microsoft.Maui.SwipeDirection']/Docs/*" />
	[Flags]
	public enum SwipeDirection
	{
		/// <include file="../../docs/Microsoft.Maui/SwipeDirection.xml" path="//Member[@MemberName='Right']/Docs/*" />
		Right = 1,
		/// <include file="../../docs/Microsoft.Maui/SwipeDirection.xml" path="//Member[@MemberName='Left']/Docs/*" />
		Left = 2,
		/// <include file="../../docs/Microsoft.Maui/SwipeDirection.xml" path="//Member[@MemberName='Up']/Docs/*" />
		Up = 4,
		/// <include file="../../docs/Microsoft.Maui/SwipeDirection.xml" path="//Member[@MemberName='Down']/Docs/*" />
		Down = 8
	}

	internal static class SwipeDirectionHelper
	{
		public static SwipeDirection GetSwipeDirection(Point initialPoint, Point endPoint)
		{
			var angle = GetAngleFromPoints(initialPoint.X, initialPoint.Y, endPoint.X, endPoint.Y);
			return GetSwipeDirectionFromAngle(angle);
		}

		internal static double GetAngleFromPoints(double x1, double y1, double x2, double y2)
		{
			double rad = Math.Atan2(y1 - y2, x2 - x1) + Math.PI;
			return (rad * 180 / Math.PI + 180) % 360;
		}

		internal static SwipeDirection GetSwipeDirectionFromAngle(double angle)
		{
			if (IsAngleInRange(angle, 45, 135))
				return SwipeDirection.Up;

			if (IsAngleInRange(angle, 0, 45) || IsAngleInRange(angle, 315, 360))
				return SwipeDirection.Right;

			if (IsAngleInRange(angle, 225, 315))
				return SwipeDirection.Down;

			return SwipeDirection.Left;
		}

		internal static bool IsAngleInRange(double angle, float init, float end)
		{
			return (angle >= init) && (angle < end);
		}
	}
}