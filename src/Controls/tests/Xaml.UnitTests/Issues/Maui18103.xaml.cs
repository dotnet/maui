using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18103 : ContentPage
{

	public Maui18103() => InitializeComponent();

	public Maui18103(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		// Constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void VSMOverride([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{
			var page = new Maui18103(useCompiledXaml);
			Assert.Equal(new SolidColorBrush(Colors.Orange), page.button.Background);
		}
	}
}