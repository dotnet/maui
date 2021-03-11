namespace Microsoft.Maui.Graphics
{
	public class SolidColorBrush : Brush
	{
		public SolidColorBrush()
		{

		}

		public SolidColorBrush(Color color)
		{
			Color = color;
		}

		public override bool IsEmpty
		{
			get
			{
				var solidColorBrush = this;
				return solidColorBrush == null || solidColorBrush.Color.IsDefault;
			}
		}

		public Color Color { get; set; } = Color.Default;

		public override bool Equals(object obj)
		{
			if (!(obj is SolidColorBrush dest))
				return false;

			return Color == dest.Color;
		}

		public override int GetHashCode()
		{
			return -1234567890 + Color.GetHashCode();
		}
	}
}