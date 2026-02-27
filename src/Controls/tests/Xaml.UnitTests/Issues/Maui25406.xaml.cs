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

	[Collection("Issue")]
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
		}

		[Fact]
		public void WhenBindingIsCompiledBindingExtensionDoesNotReceiveServiceProviderWithXamlTypeResolver()
		{
			MockCompiler.Compile(typeof(Maui25406), out var md, out bool hasLoggedErrors, generateFullIl: false);
			Assert.False(hasLoggedErrors);
#pragma warning disable xUnit2012 // Do not use Assert.False() - we're checking LINQ Any() here, not Contains
			Assert.False(md.Body.Instructions.Any(static i => i.OpCode == OpCodes.Newobj && i.Operand.ToString().Contains("XamlServiceProvider", StringComparison.Ordinal)));
#pragma warning restore xUnit2012
		}
	}
}

public class Maui25406ViewModel
{
	public string Text { get; set; }
}
