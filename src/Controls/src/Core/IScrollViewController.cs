using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
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