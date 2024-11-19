using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui23711 : ContentPage
{
	public Maui23711()
	{
		InitializeComponent();
	}

	public Maui23711(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
		public void UsesReflectionBasedBindingsWhenCompilationOfBindingsWithSourceIsDisabled([Values(false, true)] bool compileBindingsWithSource)
		{
			MockCompiler.Compile(typeof(Maui23711), out MethodDefinition methodDefinition, compileBindingsWithSource: compileBindingsWithSource);
			Assert.AreEqual(compileBindingsWithSource, ContainsTypedBindingInstantiation(methodDefinition));
		}

		static bool ContainsTypedBindingInstantiation(MethodDefinition methodDef)
			=> methodDef.Body.Instructions.Any(instruction =>
				instruction.OpCode == OpCodes.Newobj
				&& instruction.Operand is MethodReference methodRef
				&& methodRef.DeclaringType.Name == "TypedBinding`2");
	}
}

public class DeclaredModel
{
	public string Value { get; set; }
}
