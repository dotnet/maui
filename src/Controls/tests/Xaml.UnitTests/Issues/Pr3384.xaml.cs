using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Pr3384 : ContentPage
{
	public Pr3384() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
		}

		public void Dispose()
		{
			DispatcherProvider.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void RecyclingStrategyIsHandled(XamlInflator inflator)
		{
			var p = new Pr3384(inflator);
			Assert.Equal(ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
		}
	}
}