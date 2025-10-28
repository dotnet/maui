using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Pr3384 : ContentPage
	{
		public Pr3384()
		{
			InitializeComponent();
		}

		public Pr3384(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void Setup()
			{
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
				DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				DispatcherProvider.SetCurrent(null);
				DeviceInfo.SetCurrent(null);
			}

			[InlineData(false)]
			[InlineData(true)]
			public void RecyclingStrategyIsHandled(bool useCompiledXaml)
			{
				var p = new Pr3384(useCompiledXaml);
				Assert.Equal(ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
			}
		}
	}
}