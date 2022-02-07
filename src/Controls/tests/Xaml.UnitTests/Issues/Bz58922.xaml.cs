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
				DeviceInfo.SetCurrent(new Bz58922DeviceInfo(DeviceIdiom.Phone));
				var layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(320));

				DeviceInfo.SetCurrent(new Bz58922DeviceInfo(DeviceIdiom.Tablet));
				layout = new Bz58922(useCompiledXaml);
				Assert.That(layout.grid.HeightRequest, Is.EqualTo(480));
			}

			class Bz58922DeviceInfo : IDeviceInfo
			{
				public Bz58922DeviceInfo(DeviceIdiom idiom)
				{
					Idiom = idiom;
				}

				public string Model => throw new NotImplementedException();

				public string Manufacturer => throw new NotImplementedException();

				public string Name => throw new NotImplementedException();

				public string VersionString => throw new NotImplementedException();

				public Version Version => throw new NotImplementedException();

				public DevicePlatform Platform => DevicePlatform.Unknown;

				public DeviceIdiom Idiom { get; }

				public DeviceType DeviceType => DeviceType.Unknown;
			}
		}
	}
}