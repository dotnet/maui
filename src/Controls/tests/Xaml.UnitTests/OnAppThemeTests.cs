using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[Collection("Xaml Inflation")]
	public class OnAppThemeTests : BaseTestFixture
	{
		MockAppInfo mockAppInfo;
		MockApplication mockApp;

		public OnAppThemeTests()
		{
			AppInfo.SetCurrent(mockAppInfo = new MockAppInfo());
			Application.Current = mockApp = new MockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.Current = null;
				AppInfo.SetCurrent(null);
			}
			base.Dispose(disposing);
		}

		[Fact]
		public void OnAppThemeExtensionLightDarkColor()
		{
			var xaml = @"
			<Label 
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" TextColor=""{AppThemeBinding Light = Green, Dark = Red}
			"">This text is green or red depending on Light (or default) or Dark</Label>";

			SetAppTheme(AppTheme.Light);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);
			label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void OnAppThemeLightDarkColor()
		{
			var xaml = @"
			<Label
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeBinding Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

			SetAppTheme(AppTheme.Light);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);
			label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void OnAppThemeUnspecifiedThemeDefaultsToLightColor()
		{
			var xaml = @"
			<Label
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeBinding Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

			SetAppTheme(AppTheme.Unspecified);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void OnAppThemeUnspecifiedLightColorDefaultsToDefault()
		{
			var xaml = @"
			<Label
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeBinding Default=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

			SetAppTheme(AppTheme.Light);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void AppThemeColorLightDark()
		{
			var xaml = @"
			<Label
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeBinding Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

			SetAppTheme(AppTheme.Light);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);

			SetAppTheme(AppTheme.Dark);
			label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void AppThemeColorUnspecifiedThemeDefaultsToLightColor()
		{
			var xaml = @"
			<Label
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeBinding Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

			SetAppTheme(AppTheme.Unspecified);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);
		}

		[Fact]
		public void AppThemeColorUnspecifiedLightColorDefaultsToDefault()
		{
			var xaml = @"
			<Label
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeBinding Default=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

			SetAppTheme(AppTheme.Unspecified);
			var label = new Label().LoadFromXaml(xaml);
			Assert.Equal(Colors.Green, label.TextColor);
		}

		void SetAppTheme(AppTheme theme)
		{
			mockAppInfo.RequestedTheme = theme;
			mockApp.NotifyThemeChanged();
		}
	}
}