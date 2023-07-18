using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics;
using AndroidX.AppCompat.App;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class AlertDialogTests : ControlsHandlerTestBase
	{

		async Task<Color> GetDialogButtonTextColor(int nightMode)
		{
			var mauiContextStub1 = new ContextStub(ApplicationServices);
			var activity = mauiContextStub1.GetActivity();
			mauiContextStub1.Context = new Android.Views.ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);
			Color color = Color.Transparent;
			await InvokeOnMainThreadAsync(() =>
			{
				var initialNightMode = activity.Delegate.LocalNightMode;
				activity.Delegate.SetLocalNightMode(nightMode);
				var builder = new AlertDialog.Builder(activity);
				var alertDialog = builder.Create();
				alertDialog.Show();
				var button = alertDialog.GetButton((int)Android.Content.DialogButtonType.Negative);
				var textColor = button.CurrentTextColor;
				color = new Color(textColor);
				activity.Delegate.SetLocalNightMode(initialNightMode);
				alertDialog.Hide();
			});

			return color;
		}

		[Fact]
		public void AlertDialogButtonColorLightTheme()
		{
			var textColor = GetDialogButtonTextColor(AppCompatDelegate.ModeNightNo).Result;
			Assert.True(textColor.GetBrightness() < 0.5);
		}

		[Fact]
		public void AlertDialogButtonColorDarkTheme()
		{
			var textColor = GetDialogButtonTextColor(AppCompatDelegate.ModeNightYes).Result;
			Assert.True(textColor.GetBrightness() > 0.5);
		}
	}
}
