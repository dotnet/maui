using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform
{
	public interface IShellFlyoutView
	{
		AView AndroidView { get; }

		void AttachFlyout(IShellContext context, AView content);
	}
}