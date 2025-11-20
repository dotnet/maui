using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class OnPlatformTests : BaseTestFixture
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[Test]
		public void ApplyToProperty()
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var xaml = @"
			<ContentPage 
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
			xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"">
				<OnPlatform x:TypeArguments=""View"">
					<On Platform=""iOS""><Button Text=""iOS""/></On>
					<On Platform=""Android""><Button Text=""Android""/></On>
					<On Platform=""UWP""><Button Text=""UWP""/></On>
				</OnPlatform>
			</ContentPage>";
			var layout = new ContentPage().LoadFromXaml(xaml);
			Assert.NotNull(layout.Content);
		}

		[Test]
		public void UseTypeConverters()
		{
			var xaml = @"
			<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             Title=""Grid Demo Page"">
			  <ContentPage.Padding>
			    <OnPlatform x:TypeArguments=""Thickness"">
			      <On Platform=""iOS"">
			        0, 20, 0, 0
			      </On>
			      <On Platform=""Android"">
			        0, 0, 10, 0
			      </On>
			      <On Platform=""UWP"">
			        0, 20, 0, 20
			      </On>
			    </OnPlatform>
			  </ContentPage.Padding>  
			</ContentPage>";

			ContentPage layout;

			mockDeviceInfo.Platform = DevicePlatform.iOS;
			layout = new ContentPage().LoadFromXaml(xaml);
			Assert.AreEqual(new Thickness(0, 20, 0, 0), layout.Padding);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new ContentPage().LoadFromXaml(xaml);
			Assert.AreEqual(new Thickness(0, 0, 10, 0), layout.Padding);

			mockDeviceInfo.Platform = DevicePlatform.UWP;
			layout = new ContentPage().LoadFromXaml(xaml);
			Assert.AreEqual(new Thickness(0, 20, 0, 20), layout.Padding);
		}

		[Test]
		//Issue 1480
		public void TypeConverterAndDerivedTypes()
		{
			var xaml = @"
			<Image xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Image.Source>
	                <OnPlatform x:TypeArguments=""ImageSource"">
	                    <On Platform=""iOS"">icon_twitter.png</On>
	                    <On Platform=""Android"">icon_twitter.png</On>
	                    <On Platform=""UWP"">Images/icon_twitter.png</On>
	                </OnPlatform>
	            </Image.Source>
			</Image>";

			Image image;

			mockDeviceInfo.Platform = DevicePlatform.iOS;
			image = new Image().LoadFromXaml(xaml);
			Assert.AreEqual("icon_twitter.png", (image.Source as FileImageSource).File);
		}

		[Test]
		public void UWPisWinUI()
		{
			var xaml = @"
			<Image xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Image.Source>
					<OnPlatform x:TypeArguments=""ImageSource"">
						<On Platform=""iOS"">icon_twitter.png</On>
						<On Platform=""Android"">icon_twitter.png</On>
						<On Platform=""UWP"">Images/icon_twitter.png</On>
					</OnPlatform>
				</Image.Source>
			</Image>";

			Image image;

			mockDeviceInfo.Platform = DevicePlatform.WinUI;
			image = new Image().LoadFromXaml(xaml);
			Assert.AreEqual("Images/icon_twitter.png", (image.Source as FileImageSource).File);
		}

		[Test]
		public void ChecksPreferWinUI()
		{
			var xaml = @"
			<Image xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<Image.Source>
					<OnPlatform x:TypeArguments=""ImageSource"">
						<On Platform=""iOS"">icon_twitter.png</On>
						<On Platform=""Android"">icon_twitter.png</On>
						<On Platform=""UWP"">Images/icon_twitter.png</On>
						<On Platform=""WinUI"">Images/icon_twitter_preferred.png</On>
					</OnPlatform>
				</Image.Source>
			</Image>";

			Image image;

			mockDeviceInfo.Platform = DevicePlatform.WinUI;
			image = new Image().LoadFromXaml(xaml);
			Assert.AreEqual("Images/icon_twitter_preferred.png", (image.Source as FileImageSource).File);
		}

		[Test]
		public void MissingPlatformUsesDefaultValue()
		{
			// Reproduces Bugzilla39636: When MacCatalyst is not defined in OnPlatform,
			// runtime should use default(T) instead of throwing an exception
			var xaml = @"
			<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
				<ContentPage.Resources>
					<OnPlatform x:Key=""SizeMedium"" x:TypeArguments=""x:Double"">
						<On Platform=""iOS"" Value=""40""/>
						<On Platform=""Android"" Value=""30""/>
						<On Platform=""UWP"" Value=""60""/>
					</OnPlatform>
				</ContentPage.Resources>
				<Label Text=""Test"" WidthRequest=""{StaticResource SizeMedium}"" />
			</ContentPage>";

			ContentPage page;

			// Test with MacCatalyst where platform is not defined
			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			page = new ContentPage().LoadFromXaml(xaml);
			
			// Should use default value (0.0 for double) instead of throwing
			var label = page.Content as Label;
			Assert.NotNull(label);
			Assert.AreEqual(0.0, label.WidthRequest);
		}
	}

	[TestFixture]
	public class OnIdiomTests : BaseTestFixture
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[Test]
		public void StackLayoutOrientation()
		{
			var xaml = @"
			<StackLayout 
			xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
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

			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var layout = new StackLayout().LoadFromXaml(xaml);
			Assert.AreEqual(StackOrientation.Vertical, layout.Orientation);

			mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
			layout = new StackLayout().LoadFromXaml(xaml);
			Assert.AreEqual(StackOrientation.Horizontal, layout.Orientation);
		}
	}
}