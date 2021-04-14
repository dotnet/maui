using AndroidX.Core.Widget;

namespace Microsoft.Maui
{
	public static class ScrollViewExtensions
	{
		public static void UpdateContent(this NestedScrollView nativeScrollView, IScroll scrollView, ScrollViewContainer? scrollViewContainer)
		{
			if (scrollViewContainer != null)
				scrollViewContainer.ChildView = scrollView.Content;
		}
	}
}