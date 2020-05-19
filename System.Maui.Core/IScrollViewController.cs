using System;

namespace System.Maui
{
	public interface IScrollViewController : ILayoutController
	{
		Point GetScrollPositionForElement(VisualElement item, ScrollToPosition position);

		event EventHandler<ScrollToRequestedEventArgs> ScrollToRequested;

		void SendScrollFinished();

		void SetScrolledPosition(double x, double y);

		Rectangle LayoutAreaOverride { get; set; }
	}
}