namespace Microsoft.Maui
{
	public interface IButton : IView, IText
	{
		void Pressed();
		void Released();
		void Clicked();
		Thickness Padding { get; }
	}
}