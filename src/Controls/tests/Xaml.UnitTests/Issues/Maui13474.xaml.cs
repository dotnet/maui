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

public partial class Maui13474
{
	public Maui13474()
	{
		InitializeComponent();
	}

	public Maui13474(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Fact]
		public void FontImageSourceIsAppliedFromSharedResources([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui13474(useCompiledXaml);
			var fontImageSource = page.imageButton.Source as FontImageSource;
			Assert.Equal(fontImageSource.Color, Colors.Red);
			Assert.Equal(fontImageSource.FontFamily, "FontAwesome");
		}
	}
}
