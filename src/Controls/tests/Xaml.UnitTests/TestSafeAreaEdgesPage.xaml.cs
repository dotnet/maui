using System;
using System.Linq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TestSafeAreaEdgesPage : ContentPage
{
	public TestSafeAreaEdgesPage() => InitializeComponent();

	public TestSafeAreaEdgesPage(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void CompiledSafeAreaEdgesTypeConverterIsUsed()
		{
			// This test verifies that the compiled SafeAreaEdgesTypeConverter is properly registered and used
			MockCompiler.Compile(typeof(TestSafeAreaEdgesPage), out var methodDef, out var hasLoggedErrors);
			Assert.That(!hasLoggedErrors, "Compilation should succeed without errors");

			// Verify that no runtime SafeAreaEdgesTypeConverter is instantiated
			Assert.That(!methodDef.Body.Instructions.Any(instr =>
				HasConstructorForType(methodDef, instr, typeof(Microsoft.Maui.Converters.SafeAreaEdgesTypeConverter))),
				"This Xaml should not generate a new SafeAreaEdgesTypeConverter() at runtime");
		}

		static bool HasConstructorForType(Mono.Cecil.MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction, Type converterType)
		{
			if (instruction.OpCode != Mono.Cecil.Cil.OpCodes.Newobj)
				return false;
			if (instruction.Operand is not Mono.Cecil.MethodReference methodRef)
				return false;
			if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(converterType)))
				return false;
			return true;
		}
	}
}