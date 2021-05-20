using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IFrameworkElementHandler
	{
		new AView? NativeView { get; }
	}
}