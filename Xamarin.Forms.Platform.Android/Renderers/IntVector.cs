namespace Xamarin.Forms.Platform.Android
{
	internal struct IntVector
	{
		public static explicit operator IntVector(System.Drawing.Size size)
		{
			return new IntVector(size.Width, size.Height);
		}

		public static explicit operator IntVector(System.Drawing.Point point)
		{
			return new IntVector(point.X, point.Y);
		}

		public static implicit operator System.Drawing.Point(IntVector vector)
		{
			return new System.Drawing.Point(vector.X, vector.Y);
		}

		public static implicit operator System.Drawing.Size(IntVector vector)
		{
			return new System.Drawing.Size(vector.X, vector.Y);
		}

		public static bool operator ==(IntVector lhs, IntVector rhs)
		{
			return lhs.X == rhs.X && lhs.Y == rhs.Y;
		}

		public static bool operator !=(IntVector lhs, IntVector rhs)
		{
			return !(lhs == rhs);
		}

		public static System.Drawing.Rectangle operator -(System.Drawing.Rectangle source, IntVector vector) => source + -vector;

		public static System.Drawing.Rectangle operator +(System.Drawing.Rectangle source, IntVector vector) => new System.Drawing.Rectangle(source.Location + vector, source.Size);

		public static System.Drawing.Point operator -(System.Drawing.Point point, IntVector delta) => point + -delta;

		public static System.Drawing.Point operator +(System.Drawing.Point point, IntVector delta) => new System.Drawing.Point(point.X + delta.X, point.Y + delta.Y);

		public static IntVector operator -(IntVector vector, IntVector other) => vector + -other;

		public static IntVector operator +(IntVector vector, IntVector other) => new IntVector(vector.X + other.X, vector.Y + other.Y);

		public static IntVector operator -(IntVector vector) => vector * -1;

		public static IntVector operator *(IntVector vector, int scaler) => new IntVector(vector.X * scaler, vector.Y * scaler);

		public static IntVector operator /(IntVector vector, int scaler) => new IntVector(vector.X / scaler, vector.Y / scaler);

		public static IntVector operator *(IntVector vector, double scaler) => new IntVector((int)(vector.X * scaler), (int)(vector.Y * scaler));

		public static IntVector operator /(IntVector vector, double scaler) => vector * (1 / scaler);

		internal static IntVector Origin = new IntVector(0, 0);
		internal static IntVector XUnit = new IntVector(1, 0);
		internal static IntVector YUnit = new IntVector(0, 1);

		internal IntVector(int x, int y)
		{
			X = x;
			Y = y;
		}

		internal int X { get; }

		internal int Y { get; }

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override string ToString()
		{
			return $"{X},{Y}";
		}
	}
}