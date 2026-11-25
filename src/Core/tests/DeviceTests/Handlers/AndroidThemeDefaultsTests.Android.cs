using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
using ContextThemeWrapper = Android.Views.ContextThemeWrapper;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(Material3FeatureSwitchTestCollection.Name)]
	public class AndroidThemeDefaultsTests : CoreHandlerTestBase
	{
		const string Material3FeatureSwitch = "Microsoft.Maui.RuntimeFeature." + nameof(RuntimeFeature.IsMaterial3Enabled);

		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[Category(TestCategory.Label)]
		public Task LabelsRefreshDefaultsAndPreserveCustomColors(bool material3, bool materialLabel) =>
			WithThemeContexts(material3, (light, dark) =>
			{
				var label = new LabelStub { Text = "Existing label" };
				var customLabel = new LabelStub { Text = "Custom label" };
				LabelHandler handler = materialLabel ? new LabelHandler2() : new LabelHandler();
				LabelHandler customHandler = materialLabel ? new LabelHandler2() : new LabelHandler();
				handler.SetMauiContext(new MauiContext(MauiContext.Services, light));
				customHandler.SetMauiContext(new MauiContext(MauiContext.Services, light));
				handler.SetVirtualView(label);
				customHandler.SetVirtualView(customLabel);

				try
				{
					var platformView = handler.PlatformView;
					var customView = customHandler.PlatformView;
					int lightColor = platformView.CurrentTextColor;
					customView.SetTextColor(AColor.Red);

					handler.RefreshDefaultTheme(AppTheme.Dark);
					customHandler.RefreshDefaultTheme(AppTheme.Dark);

					using AppCompatTextView darkDefault = materialLabel
						? new MauiMaterialTextView(dark)
						: new MauiTextView(dark);
					Assert.NotEqual(lightColor, darkDefault.CurrentTextColor);
					AssertStateColors(darkDefault.TextColors, platformView.TextColors);
					Assert.Equal(AColor.Red.ToArgb(), customView.CurrentTextColor);
					Assert.Same(platformView, handler.PlatformView);
					Assert.Equal("Existing label", platformView.Text);

					customLabel.TextColor = Colors.Blue;
					customHandler.UpdateValue(nameof(ITextStyle.TextColor));
					customLabel.TextColor = null;
					customHandler.UpdateValue(nameof(ITextStyle.TextColor));
					AssertStateColors(darkDefault.TextColors, customView.TextColors);

					handler.RefreshDefaultTheme(AppTheme.Light);
					Assert.Equal(lightColor, platformView.CurrentTextColor);
				}
				finally
				{
					((IElementHandler)handler).DisconnectHandler();
					((IElementHandler)customHandler).DisconnectHandler();
				}
			});

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		[Category(TestCategory.Button)]
		public Task ButtonsRefreshDefaultsAndPreserveCustomColors(bool material3) =>
			WithThemeContexts(material3, (light, dark) =>
			{
				var button = new ButtonStub { Text = "Clicked 1" };
				var customButton = new ButtonStub
				{
					Text = "Custom",
					TextColor = Colors.Red,
					Background = new SolidPaintStub(Colors.Green),
				};
				var nativeButton = new ButtonStub { Text = "Native custom background" };
				var handler = new ButtonHandler();
				var customHandler = new ButtonHandler();
				var nativeHandler = new ButtonHandler();
				handler.SetMauiContext(new MauiContext(MauiContext.Services, light));
				customHandler.SetMauiContext(new MauiContext(MauiContext.Services, light));
				nativeHandler.SetMauiContext(new MauiContext(MauiContext.Services, light));
				handler.SetVirtualView(button);
				customHandler.SetVirtualView(customButton);
				nativeHandler.SetVirtualView(nativeButton);

				try
				{
					var platformView = handler.PlatformView;
					var customView = customHandler.PlatformView;
					var customBackground = customView.Background;
					int lightTextColor = platformView.CurrentTextColor;
					int lightBackgroundColor = platformView.BackgroundTintList!.DefaultColor;
					nativeHandler.PlatformView.BackgroundTintList = ColorStateList.ValueOf(AColor.Magenta);

					handler.RefreshDefaultTheme(AppTheme.Dark);
					customHandler.RefreshDefaultTheme(AppTheme.Dark);
					nativeHandler.RefreshDefaultTheme(AppTheme.Dark);

					using var darkDefault = new MauiMaterialButton(dark);
					AssertStateColors(darkDefault.TextColors, platformView.TextColors);
					AssertStateColors(darkDefault.BackgroundTintList!, platformView.BackgroundTintList!);
					Assert.Equal(AColor.Red.ToArgb(), customView.CurrentTextColor);
					Assert.Same(customBackground, customView.Background);
					Assert.Equal(AColor.Magenta.ToArgb(), nativeHandler.PlatformView.BackgroundTintList!.DefaultColor);
					Assert.Same(platformView, handler.PlatformView);
					Assert.Equal("Clicked 1", platformView.Text);

					customButton.TextColor = null;
					customButton.Background = null;
					customHandler.UpdateValue(nameof(ITextStyle.TextColor));
					customHandler.UpdateValue(nameof(IView.Background));
					AssertStateColors(darkDefault.TextColors, customView.TextColors);
					Assert.True(customView.TryGetMauiBackground(out _, out _, out var background, out _, out _));
					using var bitmap = Bitmap.CreateBitmap(20, 20, Bitmap.Config.Argb8888!);
					using var canvas = new Canvas(bitmap);
					background!.SetBounds(0, 0, 20, 20);
					background.Draw(canvas);
					int expectedColor = darkDefault.BackgroundTintList!.GetColorForState(
						[global::Android.Resource.Attribute.StateEnabled], AColor.Black);
					Assert.Equal(expectedColor, (int)bitmap.GetPixel(10, 10));

					handler.RefreshDefaultTheme(AppTheme.Light);
					Assert.Equal(lightTextColor, platformView.CurrentTextColor);
					Assert.Equal(lightBackgroundColor, platformView.BackgroundTintList!.DefaultColor);
				}
				finally
				{
					((IElementHandler)handler).DisconnectHandler();
					((IElementHandler)customHandler).DisconnectHandler();
					((IElementHandler)nativeHandler).DisconnectHandler();
				}
			});

		Task WithThemeContexts(bool material3, Action<Context, Context> action) =>
			InvokeOnMainThreadAsync(() =>
			{
				bool originalMaterial3 = RuntimeFeature.IsMaterial3Enabled;
				AppContext.SetSwitch(Material3FeatureSwitch, material3);
				try
				{
					var activity = MauiProgramDefaults.DefaultContext;
					using var lightConfiguration = new Configuration(activity.Resources!.Configuration);
					using var darkConfiguration = new Configuration(activity.Resources.Configuration);
					lightConfiguration.UiMode = (lightConfiguration.UiMode & ~UiMode.NightMask) | UiMode.NightNo;
					darkConfiguration.UiMode = (darkConfiguration.UiMode & ~UiMode.NightMask) | UiMode.NightYes;

					using var lightConfigurationContext = activity.CreateConfigurationContext(lightConfiguration);
					using var darkConfigurationContext = activity.CreateConfigurationContext(darkConfiguration);
					int style = material3
						? Resource.Style.Maui_Material3_Theme_NoActionBar
						: Resource.Style.Maui_MainTheme_NoActionBar;
					using var light = new ContextThemeWrapper(lightConfigurationContext, style);
					using var dark = new ContextThemeWrapper(darkConfigurationContext, style);
					action(light, dark);
				}
				finally
				{
					AppContext.SetSwitch(Material3FeatureSwitch, originalMaterial3);
				}
			});

		static void AssertStateColors(ColorStateList expected, ColorStateList actual)
		{
			int[][] states =
			[
				[global::Android.Resource.Attribute.StateEnabled],
				[-global::Android.Resource.Attribute.StateEnabled]
			];

			foreach (var state in states)
				Assert.Equal(expected.GetColorForState(state, AColor.Transparent), actual.GetColorForState(state, AColor.Transparent));
		}
	}
}
