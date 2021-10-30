using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class OnAppThemeTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Application.Current = new MockApplication();
		}

		[TearDown]
		public override void TearDown()
		{
			Application.Current = null;
			base.TearDown();
		}

		[Test]
		public void OnAppThemeExtensionLightDarkColor()
		{
			var xaml = @"
			<Label 
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" TextColor=""{AppThemeBinding Light = Green, Dark = Red}
			"">This text is green or red depending on Light (or default) or Dark</Label>";

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Dark;
			label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
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

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Dark;
			label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
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

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Unspecified;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);
		}

		[Test]
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

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);
		}

		[Test]
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

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Dark;
			label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
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

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Unspecified;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);
		}

		[Test]
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

			((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Unspecified;
			var label = new Label().LoadFromXaml(xaml);
			Assert.AreEqual(Colors.Green, label.TextColor);
		}
	}
}