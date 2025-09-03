using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22001
{
	public Maui22001() => InitializeComponent();

	class Test
	{
		MockDeviceDisplay mockDeviceDisplay;
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			DeviceDisplay.SetCurrent(mockDeviceDisplay = new MockDeviceDisplay());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			mockDeviceDisplay = null;
			mockDeviceInfo = null;
		}

		[Test]
		public void StateTriggerTargetName([Values] XamlInflator inflator)
		{
			var page = new Maui22001(inflator);

			IWindow window = new Window { Page = page };
			Assert.That(page._firstGrid.IsVisible, Is.True);
			Assert.That(page._secondGrid.IsVisible, Is.False);

			mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);
			Assert.That(page._firstGrid.IsVisible, Is.False);
			Assert.That(page._secondGrid.IsVisible, Is.True);
		}
	}
}
