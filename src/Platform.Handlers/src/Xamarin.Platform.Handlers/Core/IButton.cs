namespace Xamarin.Platform
{
	public interface IButton : IView, IText
	{
		void Pressed();
		void Released();
		void Clicked();
	}
}