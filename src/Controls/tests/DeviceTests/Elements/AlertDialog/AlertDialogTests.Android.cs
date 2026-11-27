using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using AndroidX.AppCompat.App;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class AlertDialogTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[Category(TestCategory.WebView)]
		public async Task FrameworkAlertDialogColorsFollowTheme(bool isDarkTheme, bool useMaterial3)
		{
			await InvokeOnMainThreadAsync(() =>
			{
				var context = MauiContext.Context;
				using var configuration = new Configuration(context.Resources.Configuration);
				configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) |
					(isDarkTheme ? UiMode.NightYes : UiMode.NightNo);
				using var configurationContext = context.CreateConfigurationContext(configuration);
				using var themedContext = new global::Android.Views.ContextThemeWrapper(
					configurationContext,
					useMaterial3 ? Resource.Style.Maui_Material3_Theme_NoActionBar : Resource.Style.Maui_MainTheme_NoActionBar);
				using var content = new global::Android.Widget.TextView(themedContext) { Text = "12 Jun 2018 19:30" };
				using var builder = new global::Android.App.AlertDialog.Builder(themedContext);
				builder.SetTitle("Set date and time");
				builder.SetView(content);
				builder.SetPositiveButton("Set", (_, _) => { });
				builder.SetNegativeButton("Cancel", (_, _) => { });
				builder.SetNeutralButton("Clear", (_, _) => { });
				using var alertDialog = builder.Create();
				alertDialog.Create();

				using var attributes = alertDialog.Context.ObtainStyledAttributes(
					new[] { global::Android.Resource.Attribute.ColorBackground, Resource.Attribute.colorOnSurface });
				var backgroundColor = new Color(attributes.GetColor(0, Color.Transparent.ToArgb()));
				var expectedTextColor = attributes.GetColor(1, Color.Transparent.ToArgb());
				Assert.True(backgroundColor.A > 0);
				Assert.NotEqual(Color.Transparent.ToArgb(), expectedTextColor);
				Assert.Equal(isDarkTheme, backgroundColor.GetBrightness() < 0.5);
				Assert.Equal(isDarkTheme, new Color(content.CurrentTextColor).GetBrightness() > 0.5);

				foreach (var buttonType in new[] { DialogButtonType.Positive, DialogButtonType.Negative, DialogButtonType.Neutral })
				{
					var button = alertDialog.GetButton((int)buttonType);
					Assert.NotNull(button);
					Assert.Equal(expectedTextColor, button.CurrentTextColor);
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
