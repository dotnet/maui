using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17461 : ContentPage
{

	public Maui17461() => InitializeComponent();

	public Maui17461(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());


		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void MissingKeyException([Values("net7.0-ios", "net7.0-android", "net7.0-macos")] string targetFramework)
		{
			MockCompiler.Compile(typeof(Maui17461), out var methodDef, targetFramework: targetFramework);
		}
	}
}