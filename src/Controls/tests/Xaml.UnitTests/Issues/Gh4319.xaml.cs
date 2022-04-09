using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh4319 : ContentPage
	{
		public Gh4319() => InitializeComponent();
		public Gh4319(bool useCompiledXaml)
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

			[TestCase(true), TestCase(false)]
			public void OnPlatformMarkupAndNamedSizes(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new Gh4319(useCompiledXaml);
				Assert.That(layout.label.FontSize, Is.EqualTo(4d));

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new Gh4319(useCompiledXaml);
				Assert.That(layout.label.FontSize, Is.EqualTo(8d));
			}
		}
	}
}