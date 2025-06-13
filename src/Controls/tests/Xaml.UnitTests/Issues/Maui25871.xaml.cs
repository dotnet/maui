using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25871 : ContentPage
{
	public Maui25871()
	{
		InitializeComponent();
	}

	public Maui25871(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	// [TestFixture] - removed for xUnit
	class Test
	{
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Fact]
		public void CompilationDoesNotFail()
		{
			MockCompiler.Compile(typeof(Maui25871));

		}
	}
}

#nullable enable
public class Maui25871ViewModel
{
}
