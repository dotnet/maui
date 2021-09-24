using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IFlyoutPageController
	{
		bool CanChangeIsPresented { get; set; }

		Rectangle DetailBounds { get; set; }

		Rectangle FlyoutBounds { get; set; }

		bool ShouldShowSplitMode { get; }

		void UpdateFlyoutLayoutBehavior();

		event EventHandler<BackButtonPressedEventArgs> BackButtonPressed;
	}
}
