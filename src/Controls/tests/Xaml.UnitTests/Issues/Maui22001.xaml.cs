using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22001
{
	public Maui22001()
	{
		InitializeComponent();
	}

	public Maui22001(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		MockDeviceDisplay mockDeviceDisplay;
		MockDeviceInfo mockDeviceInfo;
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			DeviceDisplay.SetCurrent(mockDeviceDisplay = new MockDeviceDisplay());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			mockDeviceDisplay = null;
			mockDeviceInfo = null;
		}

		[Fact]
		public void StateTriggerTargetName([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui22001(useCompiledXaml);

			IWindow window = new Window
			{
				Page = page
			};
			Assert.True(page._firstGrid.IsVisible);
			Assert.False(page._secondGrid.IsVisible);

			mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);
			Assert.False(page._firstGrid.IsVisible);
			Assert.True(page._secondGrid.IsVisible);
		}
	}
}
