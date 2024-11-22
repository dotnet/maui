using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil.Cil;
using NUnit.Framework;

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

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void WhenBindingIsCompiledBindingExtensionDoesNotReceiveServiceProviderWithXamlTypeResolver()
		{
			MockCompiler.Compile(typeof(Maui25406), out var md, generateFullIl: false);
			Assert.That(!md.Body.Instructions.Any(static i => i.OpCode == OpCodes.Newobj && i.Operand.ToString().Contains("XamlServiceProvider", StringComparison.Ordinal)));
		}
	}
}

public class Maui25406ViewModel
{
	public string Text { get; set; }
}
