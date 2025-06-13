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

	// [TestFixture] - removed for xUnit
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Fact]
		public void ObsoleteinDT([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				() => MockCompiler.Compile(typeof(Maui17484))
			else
				() => new Maui17484(useCompiledXaml)



		}
	}
}