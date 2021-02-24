using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	public interface IShellFlyoutRenderer
	{
		AView AndroidView { get; }

		void AttachFlyout(IShellContext context, AView content);
	}
}