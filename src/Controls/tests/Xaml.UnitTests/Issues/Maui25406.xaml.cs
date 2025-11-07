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


	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
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
