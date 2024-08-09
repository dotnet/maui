namespace Microsoft.Maui.AppiumTests
{
	internal struct Point
	{
		public Point(double x, double y)
		{
			X = x;
			Y = y;
		}

		public double X { get; set; }
		public double Y { get; set; }

		public override bool Equals(object? obj) => obj is Point point && Equals(point);
		public bool Equals(Point point) => X == point.X && Y == point.Y;

		public override int GetHashCode() => X.GetHashCode() ^ (Y.GetHashCode() * 397);

		public static bool operator ==(Point left, Point right) => left.Equals(right);
		public static bool operator !=(Point left, Point right) => !(left == right);
	}
}
