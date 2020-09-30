namespace Xamarin.Forms
{
	public interface IShellContentInsetObserver
	{
		void OnInsetChanged(Thickness inset, double tabThickness);
	}
}