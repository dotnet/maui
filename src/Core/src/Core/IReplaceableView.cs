using System;
namespace Microsoft.Maui
{
	// TODO ezhart This interface name is probably wrong because they want it to work for page, and page is not a view
	// (or FrameworkElement should be called View, and then it's fine)

	public interface IReplaceableView
	{
		IFrameworkElement ReplacedView { get; }
	}
}
