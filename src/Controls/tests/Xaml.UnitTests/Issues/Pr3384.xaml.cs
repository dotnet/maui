using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Devices;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
				DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			}

			[TearDown]
			public void TearDown()
			{
				DispatcherProvider.SetCurrent(null);
				DeviceInfo.SetCurrent(null);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void RecyclingStrategyIsHandled(bool useCompiledXaml)
			{
				var p = new Pr3384(useCompiledXaml);
				Assert.AreEqual(ListViewCachingStrategy.RecycleElement, p.listView.CachingStrategy);
			}
		}
	}
}