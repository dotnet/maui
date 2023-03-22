#nullable disable
using Android.App;
using Android.Content;
using Android.Media;
using Android.Views;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using AApplication = Android.App.Application;

namespace Microsoft.Maui.Controls.Platform
{
	public static class ApplicationExtensions
	{
		public static void UpdateWindowSoftInputModeAdjust(this AApplication platformView, Application application)
		{
			if (application is IApplication app)
			{
				foreach (var window in app.Windows)
					window?.Handler?.UpdateValue(PlatformConfiguration.AndroidSpecific.Application.WindowSoftInputModeAdjustProperty.PropertyName);
			}
		}

		internal static SoftInput ToPlatform(this WindowSoftInputModeAdjust windowSoftInputModeAdjust)
		{
			switch (windowSoftInputModeAdjust)
			{
				case WindowSoftInputModeAdjust.Resize:
					return SoftInput.AdjustResize;
				case WindowSoftInputModeAdjust.Unspecified:
					return SoftInput.AdjustUnspecified;
				default:
					return SoftInput.AdjustPan;
			}
		}
	}
}
