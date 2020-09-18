using System;

namespace Xamarin.Forms
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

	public interface IMasterDetailPageController : IFlyoutPageController
	{
		Rectangle MasterBounds { get; set; }
	}
}