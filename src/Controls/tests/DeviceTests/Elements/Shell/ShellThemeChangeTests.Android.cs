#nullable enable
using System;
using System.Threading.Tasks;
using Android.Content.Res;
using Android.Graphics.Drawables;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using ContextThemeWrapper = Android.Views.ContextThemeWrapper;
using NativeShellHandler = Microsoft.Maui.Controls.Handlers.ShellHandler;

namespace Microsoft.Maui.DeviceTests
{
	[CollectionDefinition(Name, DisableParallelization = true)]
	public sealed class ShellThemeChangeCollection
	{
		public const string Name = "ShellThemeChange";
	}

	[Category(TestCategory.Shell)]
	[Collection(ShellThemeChangeCollection.Name)]
	public class ShellThemeChangeTests : ControlsHandlerTestBase
	{
		const string Material3FeatureSwitch = "Microsoft.Maui.RuntimeFeature." + nameof(RuntimeFeature.IsMaterial3Enabled);

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task DefaultBackgroundFollowsMaterialThemeAndPreservesNativeChanges(bool material3) =>
			InvokeOnMainThreadAsync(() =>
			{
				bool originalMaterial3 = RuntimeFeature.IsMaterial3Enabled;
				AppContext.SetSwitch(Material3FeatureSwitch, material3);
				try
				{
					var activity = MauiProgramDefaults.DefaultContext;
					using var configuration = new Configuration(activity.Resources!.Configuration);
					configuration.UiMode = (configuration.UiMode & ~UiMode.NightMask) | UiMode.NightNo;
					using var configuredContext = activity.CreateConfigurationContext(configuration);
					int style = material3
						? Resource.Style.Maui_Material3_Theme_NoActionBar
						: Resource.Style.Maui_MainTheme_NoActionBar;
					using var context = new ContextThemeWrapper(configuredContext, style);
					using var view = new AView(context);
					var background = new ShellPageBackground();
					background.Initialize(view, context, AppTheme.Light);
					int light = ((ColorDrawable)view.Background!).Color.ToArgb();

					background.Update(view, context, AppTheme.Dark);
					int dark = ((ColorDrawable)view.Background!).Color.ToArgb();
					using var darkContext = context.CreateThemeContext(AppTheme.Dark);
					int expected = material3
						? darkContext.GetThemeAttrColor(Resource.Attribute.colorSurface)
						: ContextCompat.GetColor(context, global::Android.Resource.Color.BackgroundDark);
					Assert.NotEqual(light, dark);
					Assert.Equal(expected, dark);

					background.Update(view, context, AppTheme.Light);
					Assert.Equal(light, ((ColorDrawable)view.Background!).Color.ToArgb());
					view.SetBackgroundColor(AColor.Magenta);
					background.Update(view, context, AppTheme.Dark);
					Assert.Equal(AColor.Magenta.ToArgb(), ((ColorDrawable)view.Background!).Color.ToArgb());
				}
				finally
				{
					AppContext.SetSwitch(Material3FeatureSwitch, originalMaterial3);
				}
			});

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ThemeChangesRefreshShellDefaultsWithoutReplacingPage(bool useNewShell)
		{
			var originalApplication = await InvokeOnMainThreadAsync(() => Application.Current);
			try
			{
				var application = await InvokeOnMainThreadAsync(() =>
				{
					var app = new ApplicationStub { UserAppTheme = AppTheme.Light };
					Application.Current = app;
					return app;
				});
				EnsureHandlerCreated(builder =>
				{
					builder.Services.AddSingleton<IApplication>(application);
					builder.ConfigureMauiHandlers(handlers =>
					{
						SetupShellHandlers(handlers);
						if (useNewShell)
						{
							handlers.AddHandler<Shell, NativeShellHandler>();
							handlers.AddHandler<ShellItem, ShellItemHandler>();
							handlers.AddHandler<ShellSection, ShellSectionHandler>();
						}
					});
				});

				var (shell, page, label, button, customLabel, customButton) = await InvokeOnMainThreadAsync(() =>
				{
					var defaultLabel = new Label { Text = "Existing label" };
					var defaultButton = new Button { Text = "Clicked 0" };
					defaultButton.Clicked += (_, _) => defaultButton.Text = "Clicked 1";
					var explicitLabel = new Label { Text = "Custom", TextColor = Colors.Red };
					var explicitButton = new Button { Text = "Custom", TextColor = Colors.Red, BackgroundColor = Colors.Green };
					var contentPage = new ContentPage
					{
						Content = new VerticalStackLayout
						{
							Children = { defaultLabel, defaultButton, explicitLabel, explicitButton }
						}
					};
					var testShell = new Shell { Items = { new ShellContent { Content = contentPage } } };
					return (testShell, contentPage, defaultLabel, defaultButton, explicitLabel, explicitButton);
				});

				await CreateHandlerAndAddToWindow(shell, async () =>
				{
					await OnLoadedAsync(label);
					await OnLoadedAsync(button);
					var context = label.Handler!.MauiContext!.Context!;
					var pageView = (AView)page.Handler!.PlatformView!;
					var labelView = (AppCompatTextView)label.Handler.PlatformView!;
					var buttonView = ((ButtonHandler)button.Handler!).PlatformView;
					var customLabelView = (AppCompatTextView)customLabel.Handler!.PlatformView!;
					var customButtonView = ((ButtonHandler)customButton.Handler!).PlatformView;
					var customButtonBackground = customButtonView.Background;

					using var probe = new AView(context);
					var referenceBackground = new ShellPageBackground();
					referenceBackground.Initialize(probe, context, AppTheme.Light);
					int lightBackground = ((ColorDrawable)probe.Background!).Color.ToArgb();
					referenceBackground.Update(probe, context, AppTheme.Dark);
					int darkBackground = ((ColorDrawable)probe.Background!).Color.ToArgb();
					Assert.NotEqual(lightBackground, darkBackground);

					application.UserAppTheme = AppTheme.Dark;
					application.UserAppTheme = AppTheme.Light;
					var backgroundView = FindBackgroundView(pageView, lightBackground, useNewShell);
					int lightText = labelView.CurrentTextColor;
					buttonView.PerformClick();
					Assert.Equal("Clicked 1", button.Text);

					application.UserAppTheme = AppTheme.Dark;
					using var darkContext = context.CreateThemeContext(AppTheme.Dark);
					using var darkLabel = new MauiTextView(darkContext);
					using var darkButton = new MauiMaterialButton(darkContext);
					Assert.Equal(darkBackground, ((ColorDrawable)backgroundView.Background!).Color.ToArgb());
					Assert.NotEqual(lightText, darkLabel.CurrentTextColor);
					Assert.Equal(darkLabel.CurrentTextColor, labelView.CurrentTextColor);
					Assert.Equal(darkButton.CurrentTextColor, buttonView.CurrentTextColor);
					Assert.Equal(AColor.Red.ToArgb(), customLabelView.CurrentTextColor);
					Assert.Equal(AColor.Red.ToArgb(), customButtonView.CurrentTextColor);
					Assert.Same(customButtonBackground, customButtonView.Background);
					Assert.Same(pageView, page.Handler!.PlatformView);
					Assert.Same(labelView, label.Handler!.PlatformView);
					Assert.Same(buttonView, button.Handler!.PlatformView);
					Assert.Equal("Clicked 1", button.Text);

					application.UserAppTheme = AppTheme.Light;
					Assert.Equal(lightBackground, ((ColorDrawable)backgroundView.Background!).Color.ToArgb());
					Assert.Equal(lightText, labelView.CurrentTextColor);
					Assert.Equal("Clicked 1", button.Text);

					backgroundView.SetBackgroundColor(AColor.Magenta);
					application.UserAppTheme = AppTheme.Dark;
					Assert.Equal(AColor.Magenta.ToArgb(), ((ColorDrawable)backgroundView.Background!).Color.ToArgb());

					page.BackgroundColor = Colors.Green;
					var explicitBackground = pageView.Background;
					application.UserAppTheme = AppTheme.Light;
					Assert.Same(explicitBackground, pageView.Background);
				});
			}
			finally
			{
				await InvokeOnMainThreadAsync(() => Application.Current = originalApplication);
			}
		}

		static AView FindBackgroundView(AView pageView, int expectedColor, bool useNewShell)
		{
			for (AView? current = pageView; current is not null; current = current.Parent as AView)
			{
				if ((!useNewShell || current is FragmentContainerView) &&
					current.Background is ColorDrawable color && color.Color.ToArgb() == expectedColor)
					return current;
			}

			throw new InvalidOperationException("The Shell page background was not found.");
		}
	}
}
