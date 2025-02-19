using System;
using System.Diagnostics;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.ControlGallery;
using Microsoft.Maui.Controls.ControlGallery.Android;
using Microsoft.Maui.Controls.ControlGallery.Issues;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.Controls.ControlGallery.Android
{
	public partial class Activity1
	{
		[Export("NavigateToTest")]
		public bool NavigateToTest(string test)
		{
			return App.NavigateToTestPage(test);
		}

		[Export("Reset")]
		public void Reset()
		{
			App.Reset();
		}

		void SetUpForceRestartTest()
		{
			// Listen for messages from the app restart test
			MessagingCenter.Subscribe<RestartAppTest>(this, RestartAppTest.ForceRestart, (e) =>
			{
				// We can force a restart by making a configuration change; in this case, we'll enter
				// Car Mode. (The easy way to do this is to change the orientation, but ControlGallery
				// handles orientation changes so they don't cause a restart.)

				var uiModeManager = UiModeManager.FromContext(this);

				if (uiModeManager.CurrentModeType == UiMode.TypeCar)
				{
					// If for some reason we're already in car mode, disable it
					uiModeManager.DisableCarMode(DisableCarModeFlags.None);
				}

				uiModeManager.EnableCarMode(EnableCarModeFlags.None);

				// And put things back to normal so we can keep running tests
				uiModeManager.DisableCarMode(DisableCarModeFlags.None);

				((App)Microsoft.Maui.Controls.Application.Current).Reset();
			});
		}
	}
}

