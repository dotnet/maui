using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public interface IFlyoutPageController
	{
		bool CanChangeIsPresented { get; set; }

		Rect DetailBounds { get; set; }

		Rect FlyoutBounds { get; set; }

		bool ShouldShowSplitMode { get; }

		void UpdateFlyoutLayoutBehavior();

		event EventHandler<BackButtonPressedEventArgs> BackButtonPressed;
	}
}
