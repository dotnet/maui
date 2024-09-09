using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18324 : ContentPage
{

	public Maui18324() => InitializeComponent();

	public Maui18324(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void xTypeShoudntCrash([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(Maui18324));
			var page = new Maui18324(useCompiledXaml);
		}
	}
}