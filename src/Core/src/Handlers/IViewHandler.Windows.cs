

using Microsoft.UI.Xaml;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IViewHandler
	{
		FrameworkElement View { get; }
	}
}