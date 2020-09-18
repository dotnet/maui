namespace Xamarin.Platform
{
	public interface IButton : IText
	{
		void Pressed();
		void Released();
		void Clicked();
	}
}