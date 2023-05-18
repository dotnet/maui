#nullable disable
using AView = Android.Views.View;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public interface IShellFlyoutRenderer
	{
		AView AndroidView { get; }

		void AttachFlyout(IShellContext context, AView content);
	}
}