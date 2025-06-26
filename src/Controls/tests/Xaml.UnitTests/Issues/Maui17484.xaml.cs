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

public partial class Maui17484 : ContentPage
{

	public Maui17484() => InitializeComponent();

	public Maui17484(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	class Test
	{
		// Constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// IDisposable public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		public void ObsoleteinDT([Theory]
		[InlineData(false)]
		[InlineData(true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui17484)));
			else
				Assert.DoesNotThrow(() => new Maui17484(useCompiledXaml));

		}
	}
}