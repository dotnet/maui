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

public partial class Maui17461 : ContentPage
{

	public Maui17461() => InitializeComponent();

	public Maui17461(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());


		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
			public void Method(string targetFramework)
		{
			MockCompiler.Compile(typeof(Maui17461), out var methodDef, targetFramework: targetFramework);
		}
	}
}