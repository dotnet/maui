using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class Issue1594
	{
		[SetUp]
		public void Setup()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
		}

		[Test]
		public void OnPlatformForButtonHeight()
		{
			var xaml = @"
				<Button 
					xmlns=""http://xamarin.com/schemas/2014/forms"" 
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

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.iOS;
			var button = new Button().LoadFromXaml(xaml);
			Assert.AreEqual(33, button.HeightRequest);

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.Android;
			button = new Button().LoadFromXaml(xaml);
			Assert.AreEqual(44, button.HeightRequest);

			((MockPlatformServices)Device.PlatformServices).RuntimePlatform = Device.UWP;
			button = new Button().LoadFromXaml(xaml);
			Assert.AreEqual(44, button.HeightRequest);
		}
	}
}