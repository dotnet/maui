using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz58922 : ContentPage
	{
		public Bz58922()
		{
			InitializeComponent();
		}

		public Bz58922(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			[SetUp]
			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			[TearDown]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[TestCase(true)]
			[TestCase(false)]
			public void OnIdiomXDouble(bool useCompiledXaml)
			{
				mockDeviceInfo.Idiom = DeviceIdiom.Phone;
				var layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(320));

				mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
				layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(480));
			}
		}
	}
}