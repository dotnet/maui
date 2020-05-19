namespace System.Maui
{
	public interface IShellContentInsetObserver
	{
		void OnInsetChanged(Thickness inset, double tabThickness);
	}
}