using Android.Content;
using AView = Android.Views.View;

namespace Microsoft.Maui
{
	public interface INativeViewHandler : IFrameworkElementHandler // TODO ezhart INativeElementHandler? 
	{
		new AView? NativeView { get; } 
	}
}