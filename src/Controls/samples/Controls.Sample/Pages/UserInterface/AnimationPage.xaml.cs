using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class AnimationsPage
	{
		public AnimationsPage()
		{
			InitializeComponent();
		}

		async void OnStartAnimationButtonClicked(object sender, EventArgs e)
		{
			SetIsEnabledButtonState(false, true);

			bool isCancelled = await DotNetBotImage.TranslateTo(-100, 0, 1000);

			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateTo(-100, -100, 1000);
			}
			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateTo(100, 100, 2000);
			}
			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateTo(0, 100, 1000);
			}
			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateTo(0, 0, 1000);
			}

			SetIsEnabledButtonState(true, false);
		}

		void OnCancelAnimationButtonClicked(object sender, EventArgs e)
		{
			ViewExtensions.CancelAnimations(DotNetBotImage);
			SetIsEnabledButtonState(true, false);
		}

		void SetIsEnabledButtonState(bool startButtonState, bool cancelButtonState)
		{
			StartButton.IsEnabled = startButtonState;
			CancelButton.IsEnabled = cancelButtonState;
		}
	}
}