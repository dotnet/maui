#nullable enable
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public interface IPlatformViewHandler : IViewHandler
	{
		new FrameworkElement? PlatformView { get; }
		new FrameworkElement? ContainerView { get; }
	}
}