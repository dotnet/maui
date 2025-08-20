using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16208
{
	public Maui16208() => InitializeComponent();

	class Test
	{
		MockDeviceInfo mockDeviceInfo;
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			mockDeviceInfo = null;
		}

		[Test]
		public void SetterAndTargetName([Values] XamlInflator inflator)
		{
			Assert.DoesNotThrow(() => new Maui16208(inflator));
			var page = new Maui16208(inflator);
			Assert.That(page!.ItemLabel.BackgroundColor, Is.EqualTo(Colors.Green));
		}
	}
}