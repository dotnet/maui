namespace Microsoft.Maui
{
	public interface INativeWindowHandler : IWindowHandler
	{
		void SetWindow(UI.Xaml.Window window);
	}
}