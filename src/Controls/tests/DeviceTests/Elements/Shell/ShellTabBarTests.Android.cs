using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.TextView;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using AView = Android.Views.View;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task BottomNavigationDefaultsRefreshAfterThemeChange(bool resetAppearance)
		{
			SetupBuilder();

			await InvokeOnMainThreadAsync(() =>
			{
				const string material3Switch = "Microsoft.Maui.RuntimeFeature.IsMaterial3Enabled";
				AppContext.TryGetSwitch(material3Switch, out var originalMaterial3);
				var application = Application.Current;
				var originalTheme = application.UserAppTheme;
				try
				{
					AppContext.SetSwitch(material3Switch, true);
					application.UserAppTheme = ApplicationModel.AppTheme.Light;
					using var appContext = new AndroidX.AppCompat.View.ContextThemeWrapper(
						MauiContext.Context, Resource.Style.Maui_Material3_Theme_NoActionBar);
					using var lightContext = ShellAppearanceTheme.CreateContext(appContext);
					using var bottomView = new BottomNavigationView(lightContext);
					using var tracker = new ShellBottomNavViewAppearanceTracker(null, null);
					tracker.CaptureNativeAppearance(bottomView);
					var lightColor = bottomView.ItemTextColor.DefaultColor;
					var shell = new Shell();
					Shell.SetTabBarForegroundColor(shell, Colors.Red);
					Shell.SetTabBarUnselectedColor(shell, Colors.Blue);
					var overrides = new ShellAppearance();
					overrides.Ingest(shell);
					tracker.SetAppearance(bottomView, overrides);

					foreach (var theme in new[] { ApplicationModel.AppTheme.Dark, ApplicationModel.AppTheme.Light })
					{
						application.UserAppTheme = theme;
						using var expectedContext = ShellAppearanceTheme.CreateContext(appContext);
						using var expected = new BottomNavigationView(expectedContext);
						if (theme == ApplicationModel.AppTheme.Dark)
							Assert.NotEqual(lightColor, expected.ItemTextColor.DefaultColor);

						tracker.SetAppearance(bottomView, overrides);
						Assert.Equal(Colors.Blue.ToPlatform().ToArgb(), bottomView.ItemTextColor.DefaultColor);
						Assert.Equal(Colors.Red.ToPlatform().ToArgb(), bottomView.ItemIconTintList.GetColorForState(
							new[] { global::Android.Resource.Attribute.StateEnabled, global::Android.Resource.Attribute.StateChecked },
							global::Android.Graphics.Color.Transparent));

						if (resetAppearance)
							tracker.ResetAppearance(bottomView);
						else
							tracker.SetAppearance(bottomView, new ShellAppearance());

						Assert.Equal(expected.ItemTextColor.DefaultColor, bottomView.ItemTextColor.DefaultColor);
						Assert.Equal(expected.ItemIconTintList.DefaultColor, bottomView.ItemIconTintList.DefaultColor);
					}
				}
				finally
				{
					application.UserAppTheme = originalTheme;
					AppContext.SetSwitch(material3Switch, originalMaterial3);
				}
			});
		}

		BottomNavigationView GetTab(ShellSection item)
		{
			var shell = item.FindParentOfType<Shell>();
			var renderer = (ShellRenderer)shell.Handler;
			var bottomView = GetDrawerLayout(renderer).GetFirstChildOfType<BottomNavigationView>();
			var menu = bottomView.Menu;
			var index = shell.CurrentItem.Items.IndexOf(item);

			if (index < 0 || index >= menu.Size())
				Assert.Fail("ShellSection not found in Shell");

			if (index >= menu.Size())
				Assert.Fail("Menu Item has not been created for this item");

			return bottomView;
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTab(item),
					item.Title, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTab(item),
					item.Title, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
				ShellSection item,
				Color textColor,
				bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTab(item),
					item.Title, textColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTab(item),
					item.Title, textColor, MauiContext);
			}
		}
	}
}
