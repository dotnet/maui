using System;
using Microsoft.Maui;
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

			bool isCancelled = await DotNetBotImage.TranslateToAsync(-100, 0, 1000);

			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateToAsync(-100, -100, 1000);
			}
			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateToAsync(100, 100, 2000);
			}
			if (!isCancelled)
			{
				isCancelled = await DotNetBotImage.TranslateToAsync(0, 100, 1000);
			}
			if (!isCancelled)
			{
				await DotNetBotImage.TranslateToAsync(0, 0, 1000);
			}

			SetIsEnabledButtonState(true, false);
		}

		void OnStartCustomAnimationButtonClicked(object sender, EventArgs e)
		{
			var parentAnimation = new Animation();
			var scaleUpAnimation = new Animation(v => DotNetBotImage.Scale = v, 1, 2, Easing.SpringIn);
			var rotateAnimation = new Animation(v => DotNetBotImage.Rotation = v, 0, 360);
			var scaleDownAnimation = new Animation(v => DotNetBotImage.Scale = v, 2, 1, Easing.SpringOut);

			parentAnimation.Add(0, 0.5, scaleUpAnimation);
			parentAnimation.Add(0, 1, rotateAnimation);
			parentAnimation.Add(0.5, 1, scaleDownAnimation);

			parentAnimation.Commit(this, "CustomAnimation", 16, 4000, null, (v, c) => SetIsEnabledButtonState(true, false));
		}

		void OnCancelAnimationButtonClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(DotNetBotImage);
			SetIsEnabledButtonState(true, false);
		}

		void SetIsEnabledButtonState(bool startButtonState, bool cancelButtonState)
		{
			StartButton.IsEnabled = startButtonState;
			CancelButton.IsEnabled = cancelButtonState;
		}
	}
}