using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class FontSize : ContentPage
	{
		public FontSize() =>
			InitializeComponent();

		public FontSize(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Fact]
			public void FontSizeExtensionsAreReplaced()
			{
				MockCompiler.Compile(typeof(FontSize), out var methodDef, out var hasLoggedErrors);
				Assert.That(!hasLoggedErrors);
				Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsFontSizeConverterCtor(methodDef, instr)), "This Xaml still generates a new FontSizeConverter()");
			}

			bool InstructionIsFontSizeConverterCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
			{
				if (instruction.OpCode != OpCodes.Newobj)
					return false;
				if (!(instruction.Operand is MethodReference methodRef))
					return false;
				if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(Microsoft.Maui.Controls.FontSizeConverter))))
					return false;
				return true;
			}

			[Fact]
			public void CorrectFontSizes([Values(false, true)] bool useCompiledXaml)
			{
				var page = new FontSize(useCompiledXaml);
				Assert.Equal(42, page.l42.FontSize);
#pragma warning disable CS0612 // Type or member is obsolete
				Assert.Equal(Device.GetNamedSize(NamedSize.Medium, page.lmedium, page.lmedium.FontSize);
				Assert.Equal(Device.GetNamedSize(NamedSize.Default, page.ldefault, page.ldefault.FontSize);
				Assert.Equal(Device.GetNamedSize(NamedSize.Default, page.bdefault, page.bdefault.FontSize);
#pragma warning restore CS0612 // Type or member is obsolete

			}
		}
	}
}
