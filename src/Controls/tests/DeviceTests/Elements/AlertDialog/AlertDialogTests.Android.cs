using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using AndroidX.AppCompat.App;
using AndroidX.Core.Graphics;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
using FrameworkAlertDialog = Android.App.AlertDialog;

namespace Microsoft.Maui.DeviceTests
{
	public partial class AlertDialogTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		[Category(TestCategory.WebView)]
		public async Task FrameworkDialogButtonsHaveSufficientContrast(bool isDarkTheme)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var mauiContext = new ContextStub(ApplicationServices);
				var activity = mauiContext.GetActivity();
				using var configuration = new Configuration(activity.Resources?.Configuration);
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(isDarkTheme ? UiMode.NightYes : UiMode.NightNo);
				using var configurationContext = activity.CreateConfigurationContext(configuration);
				using var themedContext = new global::Android.Views.ContextThemeWrapper(
					configurationContext, Resource.Style.Maui_MainTheme_NoActionBar);
				using var builder = new FrameworkAlertDialog.Builder(themedContext);
				using var dialog = builder
					.SetTitle("Set date and time")
					.SetPositiveButton("Set", (_, _) => { })
					.SetNegativeButton("Cancel", (_, _) => { })
					.SetNeutralButton("Clear", (_, _) => { })
					.Create();
				dialog.Create();

				var background = dialog.Window?.DecorView.Background;
				Assert.NotNull(background);
				using var bitmap = Bitmap.CreateBitmap(256, 256, Bitmap.Config.Argb8888);
				using var canvas = new Canvas(bitmap);
				background.SetBounds(0, 0, bitmap.Width, bitmap.Height);
				background.Draw(canvas);
				var backgroundColor = bitmap.GetPixel(bitmap.Width / 2, bitmap.Height / 2);
				Assert.Equal(255, new AColor(backgroundColor).A);
				Assert.Equal(isDarkTheme, ColorUtils.CalculateLuminance(backgroundColor) < 0.5);

				foreach (var buttonType in new[] { DialogButtonType.Positive, DialogButtonType.Negative, DialogButtonType.Neutral })
				{
					var button = dialog.GetButton((int)buttonType);
					Assert.NotNull(button);
					var contrast = ColorUtils.CalculateContrast(button.CurrentTextColor, backgroundColor);
					Assert.True(contrast >= 4.5,
						$"{buttonType} button contrast was {contrast:F2}:1 in {(isDarkTheme ? "dark" : "light")} theme.");
				}
			});
		}

		async Task<Color> GetDialogButtonTextColor(int nightMode)
		{
			var mauiContextStub1 = new ContextStub(ApplicationServices);
			var activity = mauiContextStub1.GetActivity();
			mauiContextStub1.Context = new global::Android.Views.ContextThemeWrapper(activity, Resource.Style.Maui_MainTheme_NoActionBar);
			Color color = Color.Transparent;
			await InvokeOnMainThreadAsync(() =>
			{
				var initialNightMode = activity.Delegate.LocalNightMode;
				activity.Delegate.SetLocalNightMode(nightMode);
				var builder = new AlertDialog.Builder(activity);
				var alertDialog = builder.Create();
				alertDialog.Show();
				var button = alertDialog.GetButton((int)global::Android.Content.DialogButtonType.Negative);
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
