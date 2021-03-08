namespace Microsoft.Maui
{
	public interface IButton : IView, IText, IPadding
	{
		void Pressed();
		void Released();
		void Clicked();
	}
}