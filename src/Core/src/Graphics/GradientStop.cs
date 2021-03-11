namespace Microsoft.Maui.Graphics
{
	public class GradientStop
	{
		public Color Color { get; set; }

		public float Offset { get; set; }

		public GradientStop() { }

		public GradientStop(Color color, float offset)
		{
			Color = color;
			Offset = offset;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is GradientStop dest))
				return false;

			return Color == dest.Color && System.Math.Abs(Offset - dest.Offset) < 0.00001;
		}

		public override int GetHashCode()
		{
			return -1234567890 + Color.GetHashCode();
		}
	}
}