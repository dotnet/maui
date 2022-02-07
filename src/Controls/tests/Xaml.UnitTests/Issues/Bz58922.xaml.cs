using System;
using Microsoft.Maui.Essentials;
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
			[TearDown]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
				Device.PlatformServices = null;
			}

			[TestCase(true)]
			[TestCase(false)]
			public void OnIdiomXDouble(bool useCompiledXaml)
			{
				DeviceInfo.SetCurrent(new MockDeviceInfo(DeviceIdiom.Phone));
				var layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(320));

				DeviceInfo.SetCurrent(new MockDeviceInfo(DeviceIdiom.Tablet));
				layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(480));
			}
		}
	}
}