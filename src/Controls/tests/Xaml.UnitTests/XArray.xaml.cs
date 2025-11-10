using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[ContentProperty("Content")]
public class MockBindableForArray : View
{
	public object Content { get; set; }
}

public partial class XArray : MockBindableForArray
{
	public XArray() => InitializeComponent();


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void SupportsXArray(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation().WithAdditionalSource(
"""
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[ContentProperty("Content")]
public class MockBindableForArray : View
{
	public object Content { get; set; }
}

[XamlProcessing(XamlInflator.Default, true)]
public partial class XArray : MockBindableForArray
{
	public XArray() => InitializeComponent();
}
""").RunMauiSourceGenerator(typeof(XArray));
				Assert.Empty(result.Diagnostics);
				return;
			}
			var layout = new XArray(inflator);
			var array = layout.Content;
			Assert.NotNull(array);
			Assert.IsType<string[]>(array);
			Assert.Equal(2, ((string[])layout.Content).Length);
			Assert.Equal("Hello", ((string[])layout.Content)[0]);
			Assert.Equal("World", ((string[])layout.Content)[1]);
		}

		[Fact]
		public void ArrayExtensionNotPresentInGeneratedCode()
		{
			MockCompiler.Compile(typeof(XArray), out var methodDef, out var hasLoggedErrors);
			Assert.True(!hasLoggedErrors);
			Assert.True(!methodDef.Body.Instructions.Any(instr => InstructionIsArrayExtensionCtor(methodDef, instr)), "This Xaml still generates a new ArrayExtension()");
		}

		static bool InstructionIsArrayExtensionCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
		{
			if (instruction.OpCode != OpCodes.Newobj)
				return false;
			if (instruction.Operand is not MethodReference methodRef)
				return false;
			if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(ArrayExtension))))
				return false;
			return true;
		}
	}
}