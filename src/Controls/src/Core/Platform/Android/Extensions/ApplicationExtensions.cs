using Android.App;
using Android.Content;
using Android.Views;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using AApplication = Android.App.Application;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ApplicationExtensions
	{
		public static void UpdateWindowSoftInputModeAdjust(this AApplication platformView, Application application)
		{
			var adjust = SoftInput.AdjustPan;

			if (Application.Current != null)
			{
				WindowSoftInputModeAdjust elementValue = Application.Current.OnThisPlatform().GetWindowSoftInputModeAdjust();

				switch (elementValue)
				{
					case WindowSoftInputModeAdjust.Resize:
						adjust = SoftInput.AdjustResize;
						break;
					case WindowSoftInputModeAdjust.Unspecified:
						adjust = SoftInput.AdjustUnspecified;
						break;
					default:
						adjust = SoftInput.AdjustPan;
						break;
				}
			}

			IMauiContext mauiContext = application.FindMauiContext(true);
			Context context = mauiContext?.Context;
			Activity activity = context.GetActivity();

			activity?.Window?.SetSoftInputMode(adjust);

		}
	}
}
