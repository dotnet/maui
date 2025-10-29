using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Issue1594
	{
		MockDeviceInfo mockDeviceInfo;

		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		[Xunit.Fact]
		public void Setup()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		[Xunit.Fact]
		public void TearDown()
		{
			DeviceInfo.SetCurrent(null);
		}

		[Fact]
		public void OnPlatformForButtonHeight()
		{
			var xaml = @"
				<Button 
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui"" 
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" 
					xmlns:sys=""clr-namespace:System;assembly=mscorlib""
					x:Name=""activateButton"" Text=""ACTIVATE NOW"" TextColor=""White"" BackgroundColor=""#00A0FF"">
				        <Button.HeightRequest>
				           <OnPlatform x:TypeArguments=""sys:Double"">
				                   <On Platform=""iOS"">33</On>
				                   <On Platform=""Android"">44</On>
				                   <On Platform=""UWP"">44</On>
				         	</OnPlatform>
				         </Button.HeightRequest>
				 </Button>";

			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var button = new Button().LoadFromXaml(xaml);
			Assert.Equal(33, button.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			button = new Button().LoadFromXaml(xaml);
			Assert.Equal(44, button.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.UWP;
			button = new Button().LoadFromXaml(xaml);
			Assert.Equal(44, button.HeightRequest);
		}
	}
}