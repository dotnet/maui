using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22877 : ContentPage
{
	public Maui22877() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnBindingRelease(XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var page = new Maui22877(inflator) { BindingContext = new { BoundString = "BoundString" } };
			Assert.Equal("Grade", page.label0.Text);

			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			page = new Maui22877(inflator) { BindingContext = new { BoundString = "BoundString" } };
			Assert.Equal("BoundString", page.label0.Text);


		}
	}
}