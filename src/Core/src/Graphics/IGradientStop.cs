namespace Microsoft.Maui.Graphics
{
	public interface IGradientStop
	{
		Color Color { get; set; }

		float Offset { get; set; }
	}
}