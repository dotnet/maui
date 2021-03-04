namespace Microsoft.Maui
{
	public interface IActivityIndicator : IView
	{
		bool IsRunning { get; }
		Color Color { get; }
	}
}