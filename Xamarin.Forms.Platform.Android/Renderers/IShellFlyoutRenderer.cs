using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public interface IShellFlyoutRenderer
	{
		AView AndroidView { get; }

		void AttachFlyout(IShellContext context, AView content);
	}
}