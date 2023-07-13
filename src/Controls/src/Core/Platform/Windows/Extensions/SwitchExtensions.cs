using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	public static class SwitchExtensions
	{
		static object? OnContent;
		static object? OffContent;

		public static void UpdateShowStatusLabel(this ToggleSwitch platformView, Switch switchControl,
			bool shouldShow)
		{
			// Save the platform default objects so we can restore it later
			if (OnContent is null && platformView.OnContent is not null)
			{
				OnContent = platformView.OnContent;
			}

			if (OffContent is null && platformView.OffContent is not null)
			{
				OffContent = platformView.OffContent;
			}

			if (shouldShow)
			{
				platformView.OnContent = OnContent;
				platformView.OffContent = OffContent;

				return;
			}

			platformView.OnContent = platformView.OffContent = null;
		}
	}
}
