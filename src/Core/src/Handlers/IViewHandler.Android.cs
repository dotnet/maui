using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new AView? PlatformView { get; }

		new AView? ContainerView { get; }
	}
}