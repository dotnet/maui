using AView = Android.Views.View;

namespace System.Maui.Platform.Android
{
	public interface IShellFlyoutRenderer
	{
		AView AndroidView { get; }

		void AttachFlyout(IShellContext context, AView content);
	}
}