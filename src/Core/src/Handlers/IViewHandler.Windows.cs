#nullable enable
using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IFrameworkElementHandler
	{
		new FrameworkElement? NativeView { get; }
	}
}