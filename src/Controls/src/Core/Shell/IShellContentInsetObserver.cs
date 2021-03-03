namespace Microsoft.Maui.Controls
{
	public interface IShellContentInsetObserver
	{
		void OnInsetChanged(Thickness inset, double tabThickness);
	}
}