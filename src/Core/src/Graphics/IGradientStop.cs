namespace Microsoft.Maui.Graphics
{
	public interface IGradientStop
	{
		Color Color { get; }

		float Offset { get; }
	}
}