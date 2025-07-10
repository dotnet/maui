using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz44213 : ContentPage
	{
		public Bz44213()
		{
			InitializeComponent();
		}

		public Bz44213(bool useCompiledXaml)
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
			public void BindingInOnPlatform(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var p = new Bz44213(useCompiledXaml);
				p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
				Assert.AreEqual("Foo", p.label.Text);
				mockDeviceInfo.Platform = DevicePlatform.Android;
				p = new Bz44213(useCompiledXaml);
				p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
				Assert.AreEqual("Bar", p.label.Text);
			}
		}
	}
}