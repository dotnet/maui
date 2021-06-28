using Android.App;

namespace Microsoft.Maui
{
	public interface INativeWindowHandler : IWindowHandler
	{
		void SetWindow(Activity activity);
	}
}