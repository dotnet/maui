#nullable enable
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		new FrameworkElement? NativeView { get; }
		new FrameworkElement? ContainerView { get; }
	}
}