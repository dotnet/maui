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

public partial class Maui17950 : ContentPage
{

	public Maui17950() => InitializeComponent();

	public Maui17950(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	void TestBtn(object sender, EventArgs e)
	{
		Console.WriteLine("event called");
		Assert.Pass();
	}

	void TestBtn(object sender, string e)
	{
		Console.WriteLine("wrong event called");
		throw new Xunit.Sdk.XunitException("wrong method invoked");
	}

	void TestBtn()
	{
		Console.WriteLine("normal method called");
		throw new Xunit.Sdk.XunitException("wrong method invoked");
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui17950)));
			Maui17950 page = new Maui17950(useCompiledXaml);
			page.button.SendClicked();
			throw new Xunit.Sdk.XunitException("no method invoked");
		}
	}
}