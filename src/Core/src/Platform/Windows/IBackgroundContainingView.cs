using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Platform
{
	internal interface IBackgroundContainingView
	{
		FrameworkElement BackgroundHost { get; }
	}
}
