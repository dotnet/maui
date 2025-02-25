using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui21038
{
	public Maui21038()
	{
		InitializeComponent();
	}

	public Maui21038(bool useCompiledXaml)
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
		public void XamlParseErrorsHaveFileInfo([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				MockCompiler.Compile(typeof(Maui21038), out var md, out var hasLoggedErrors);
				Assert.That(hasLoggedErrors);
			}
			else
				Assert.Throws<XamlParseException>(() => new Maui21038(useCompiledXaml));
		}
	}
}
