using NUnit.Framework;
using System;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class OnPlatformTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			Device.PlatformServices = null;
			base.TearDown();
		}

		[Test]
		public void ApplyToProperty ()
		{
			var xaml = @"
			<ContentPage 
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"">
				<OnPlatform x:TypeArguments=""View"">
					<OnPlatform.iOS><Button Text=""iOS""/></OnPlatform.iOS>
					<OnPlatform.Android><Button Text=""Android""/></OnPlatform.Android>
					<OnPlatform.WinPhone><Button Text=""WinPhone""/></OnPlatform.WinPhone>
				</OnPlatform>
			</ContentPage>";
			var layout = new ContentPage ().LoadFromXaml (xaml);
			Assert.NotNull (layout.Content);
		}

		[Test]
		public void UseTypeConverters ()
		{
			var xaml = @"
			<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             Title=""Grid Demo Page"">
			  <ContentPage.Padding>
			    <OnPlatform x:TypeArguments=""Thickness"">
			      <OnPlatform.iOS>
			        0, 20, 0, 0
			      </OnPlatform.iOS>
			      <OnPlatform.Android>
			        0, 0, 10, 0
			      </OnPlatform.Android>
			      <OnPlatform.WinPhone>
			        0, 20, 0, 20
			      </OnPlatform.WinPhone>
			    </OnPlatform>
			  </ContentPage.Padding>  
			</ContentPage>";

			ContentPage layout;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
			layout = new ContentPage ().LoadFromXaml (xaml);
			Assert.AreEqual (new Thickness (0, 20, 0, 0), layout.Padding);

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
			layout = new ContentPage ().LoadFromXaml (xaml);
			Assert.AreEqual (new Thickness (0, 0, 10, 0), layout.Padding);

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.UWP;
			layout = new ContentPage ().LoadFromXaml (xaml);
			Assert.AreEqual (new Thickness (0, 20, 0, 20), layout.Padding);
		}

		[Test]
		//Issue 1480
		public void TypeConverterAndDerivedTypes ()
		{
			var xaml = @"
			<Image xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Image.Source>
	                <OnPlatform x:TypeArguments=""ImageSource"">
	                    <OnPlatform.iOS>icon_twitter.png</OnPlatform.iOS>
	                    <OnPlatform.Android>icon_twitter.png</OnPlatform.Android>
	                    <OnPlatform.WinPhone>Images/icon_twitter.png</OnPlatform.WinPhone>
	                </OnPlatform>
	            </Image.Source>
			</Image>";

			Image image;

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
			image = new Image ().LoadFromXaml (xaml);
			Assert.AreEqual ("icon_twitter.png", (image.Source as FileImageSource).File);
		}
	}

	[TestFixture]
	public class OnIdiomTests : BaseTestFixture
	{
		[Test]
		public void StackLayoutOrientation ()
		{
			var xaml = @"
			<StackLayout 
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<StackLayout.Orientation>
				<OnIdiom x:TypeArguments=""StackOrientation"">
					<OnIdiom.Phone>Vertical</OnIdiom.Phone>
					<OnIdiom.Tablet>Horizontal</OnIdiom.Tablet>
				</OnIdiom>
				</StackLayout.Orientation>
				<Label Text=""child0""/>
				<Label Text=""child1""/>			
			</StackLayout>";
			Device.Idiom = TargetIdiom.Phone;
			var layout = new StackLayout ().LoadFromXaml (xaml);
			Assert.AreEqual (StackOrientation.Vertical, layout.Orientation);

			Device.Idiom = TargetIdiom.Tablet;
			layout = new StackLayout ().LoadFromXaml (xaml);
			Assert.AreEqual (StackOrientation.Horizontal, layout.Orientation);
		}
	}

	[TestFixture]
	public class OnAppThemeTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.SetFlags(new[] { "AppTheme_Experimental" });
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
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" TextColor=""{OnAppTheme Light = Green, Dark = Red}
			"">This text is green or red depending on Light (or default) or Dark</Label>";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Dark;
            label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Red, label.TextColor);
        }

		[Test]
		public void OnAppThemeLightDarkColor()
		{
			var xaml = @"
			<Label
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <OnAppTheme x:DataType=""Color"" Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Dark;
            label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Red, label.TextColor);
        }

		[Test]
		public void OnAppThemeUnspecifiedThemeDefaultsToLightColor()
		{
			var xaml = @"
			<Label
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <OnAppTheme x:DataType=""Color"" Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Unspecified;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);
        }

		[Test]
		public void OnAppThemeUnspecifiedLightColorDefaultsToDefault()
		{
			var xaml = @"
			<Label
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <OnAppTheme x:DataType=""Color"" Default=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);
        }

		[Test]
		public void AppThemeColorLightDark()
		{
			var xaml = @"
			<Label
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeColor Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Light;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Dark;
            label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Red, label.TextColor);
        }

		[Test]
		public void AppThemeColorUnspecifiedThemeDefaultsToLightColor()
		{
			var xaml = @"
			<Label
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeColor Light=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Unspecified;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);
        }

		[Test]
		public void AppThemeColorUnspecifiedLightColorDefaultsToDefault()
		{
			var xaml = @"
			<Label
			xmlns=""http://xamarin.com/schemas/2014/forms""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			Text=""This text is green or red depending on Light(or default) or Dark"">
                <Label.TextColor>
                    <AppThemeColor Default=""Green"" Dark=""Red"" />
				</Label.TextColor>
			</Label> ";

            ((MockPlatformServices)Device.PlatformServices).RequestedTheme = OSAppTheme.Unspecified;
            var label = new Label().LoadFromXaml(xaml);
            Assert.AreEqual(Color.Green, label.TextColor);
        }
    }
}