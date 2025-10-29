using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui25406 : ContentPage
{
	public Maui25406()
	{
		InitializeComponent();
	}

	public Maui25406(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Fact]
		public void WhenBindingIsCompiledBindingExtensionDoesNotReceiveServiceProviderWithXamlTypeResolver()
		{
			MockCompiler.Compile(typeof(Maui25406), out var md, out bool hasLoggedErrors, generateFullIl: false);
			Assert.True(!hasLoggedErrors);
			Assert.True(!md.Body.Instructions.Any(static i => i.OpCode == OpCodes.Newobj && i.Operand.ToString().Contains("XamlServiceProvider", StringComparison.Ordinal)));
		}
	}
}

public class Maui25406ViewModel
{
	public string Text { get; set; }
}
