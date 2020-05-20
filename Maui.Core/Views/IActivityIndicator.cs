namespace System.Maui
{
	public interface IActivityIndicator : IView
	{
		bool IsRunning { get; }
		Color Color { get; }
	}
}