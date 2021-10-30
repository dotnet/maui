using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		new AView? NativeView { get; }

		new AView? ContainerView { get; }
	}
}