using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

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

		[TestFixture]
		public class Tests
		{
			[Test]
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

			[Test]
			public void CorrectFontSizes([Values(false, true)] bool useCompiledXaml)
			{
				var page = new FontSize(useCompiledXaml);
				Assert.That(page.l42.FontSize, Is.EqualTo(42));

#pragma warning disable CS0612 // Type or member is obsolete
				Assert.That(page.lmedium.FontSize, Is.EqualTo(Device.GetNamedSize(NamedSize.Medium, page.lmedium)));
				Assert.That(page.ldefault.FontSize, Is.EqualTo(Device.GetNamedSize(NamedSize.Default, page.ldefault)));
				Assert.That(page.bdefault.FontSize, Is.EqualTo(Device.GetNamedSize(NamedSize.Default, page.bdefault)));
#pragma warning restore CS0612 // Type or member is obsolete

			}
		}
	}
}
