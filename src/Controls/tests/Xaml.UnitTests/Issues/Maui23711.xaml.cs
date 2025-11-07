using System;
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

public partial class Maui23711 : ContentPage
{
	public Maui23711()
	{
		InitializeComponent();
	}


	public class Test : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void UsesReflectionBasedBindingsWhenCompilationOfBindingsWithSourceIsDisabled(XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(Maui23711), out MethodDefinition methodDefinition, out bool hasLoggedErrors, compileBindingsWithSource: inflator == XamlInflator.XamlC);
			Assert.True(!hasLoggedErrors);
			Assert.Equal(inflator == XamlInflator.XamlC, ContainsTypedBindingInstantiation(methodDefinition));
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
