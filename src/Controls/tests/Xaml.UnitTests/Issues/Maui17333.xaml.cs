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

public partial class Maui17333 : ResourceDictionary
{

	public Maui17333() => InitializeComponent();

	public Maui17333(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void CompilerDoesntThrowOnOnPlatform([Values(true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				MockCompiler.Compile(typeof(Maui17333), targetFramework: "net-ios");
			}
		}
	}
}