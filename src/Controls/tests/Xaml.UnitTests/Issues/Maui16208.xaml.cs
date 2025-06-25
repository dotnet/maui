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

public partial class Maui16208
{
	public Maui16208()
	{
		InitializeComponent();
	}

	public Maui16208(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		MockDeviceInfo mockDeviceInfo;
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			mockDeviceInfo = null;
		}

		[Fact]
		public void SetterAndTargetName([Values(false, true)] bool useCompiledXaml)
		{

			Assert.DoesNotThrow(() => new Maui16208(useCompiledXaml));
			var page = new Maui16208(useCompiledXaml);
			Assert.Equal(Colors.Green, page!.ItemLabel.BackgroundColor);
		}
	}
}