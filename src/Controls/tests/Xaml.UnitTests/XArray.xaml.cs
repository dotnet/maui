using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[ContentProperty("Content")]
	public class MockBindableForArray : View
	{
		public object Content { get; set; }
	}

	public partial class XArray : MockBindableForArray
	{
		public XArray()
		{
			InitializeComponent();
		}

		public XArray(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public void SupportsXArray(bool useCompiledXaml)
			{
				var layout = new XArray(useCompiledXaml);
				var array = layout.Content;
				Assert.NotNull(array);
				Assert.That(array, Is.TypeOf<string[]>());
				Assert.AreEqual(2, ((string[])layout.Content).Length);
				Assert.AreEqual("Hello", ((string[])layout.Content)[0]);
				Assert.AreEqual("World", ((string[])layout.Content)[1]);
			}

			[Test]
			public void ArrayExtensionNotPresentInGeneratedCode([Values(false)] bool useCompiledXaml)
			{
				MockCompiler.Compile(typeof(XArray), out var methodDef, out var hasLoggedErrors);
				Assert.That(!hasLoggedErrors);
				Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsArrayExtensionCtor(methodDef, instr)), "This Xaml still generates a new ArrayExtension()");
			}

			bool InstructionIsArrayExtensionCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
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
}