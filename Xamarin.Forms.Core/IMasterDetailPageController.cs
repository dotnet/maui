using System;

namespace Xamarin.Forms
{
	public interface IMasterDetailPageController
	{
		bool CanChangeIsPresented { get; set; }

		Rectangle DetailBounds { get; set; }

		Rectangle MasterBounds { get; set; }

		bool ShouldShowSplitMode { get; }

		void UpdateMasterBehavior();

		event EventHandler<BackButtonPressedEventArgs> BackButtonPressed;
	}
}