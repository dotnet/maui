using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz58922 : ContentPage
{
	public Bz58922()
	{
		InitializeComponent();
	}


	[Collection("Issue")]
	public class Tests : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void OnIdiomXDouble(XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var layout = new Bz58922(inflator);
			Assert.Equal(320, layout.grid.HeightRequest);

			mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
			layout = new Bz58922(inflator);
			Assert.Equal(480, layout.grid.HeightRequest);
		}
	}
}