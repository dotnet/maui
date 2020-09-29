using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
				           <OnPlatform x:TypeArguments=""sys:Double""
				                   iOS=""33""
				                   Android=""44""
				                   WinPhone=""44"" />
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