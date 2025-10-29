using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void SupportsXArray([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation().WithAdditionalSource(
"""
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

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
				Assert.That(result.Diagnostics, Is.Empty, "No diagnostics expected for XArray");
				return;
			}
			var layout = new XArray(inflator);
			var array = layout.Content;
			Assert.NotNull(array);
			Assert.That(array, Is.TypeOf<string[]>());
			Assert.AreEqual(2, ((string[])layout.Content).Length);
			Assert.AreEqual("Hello", ((string[])layout.Content)[0]);
			Assert.AreEqual("World", ((string[])layout.Content)[1]);
		}

		[Test]
		public void ArrayExtensionNotPresentInGeneratedCode()
		{
			MockCompiler.Compile(typeof(XArray), out var methodDef, out var hasLoggedErrors);
			Assert.That(!hasLoggedErrors);
			Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsArrayExtensionCtor(methodDef, instr)), "This Xaml still generates a new ArrayExtension()");
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