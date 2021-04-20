namespace Microsoft.Maui.Graphics
{
	public interface IPattern
	{
		float Width { get; }
		float Height { get; }
		float StepX { get; }
		float StepY { get; }
		void Draw(ICanvas canvas);
	}
}
