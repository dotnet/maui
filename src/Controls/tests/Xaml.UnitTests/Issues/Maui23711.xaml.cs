using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

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
	public class Test
	{
		MockDeviceInfo mockDeviceInfo;

		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		public void Method(bool compileBindingsWithSource)
		{
			MockCompiler.Compile(typeof(Maui23711), out MethodDefinition methodDefinition, out bool hasLoggedErrors, compileBindingsWithSource: compileBindingsWithSource);
			Assert.True(!hasLoggedErrors);
			Assert.Equal(compileBindingsWithSource, ContainsTypedBindingInstantiation(methodDefinition));
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
