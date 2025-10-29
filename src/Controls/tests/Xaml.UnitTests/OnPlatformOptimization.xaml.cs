using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class OnPlatformOptimization : ContentPage
	{
		public OnPlatformOptimization()
		{
			InitializeComponent();
		}

		public OnPlatformOptimization(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[Theory]
			public void Method(string targetFramework)
			{
				MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, out var hasLoggedErrors, targetFramework);
				Assert.True(!hasLoggedErrors);
				Assert.True(!methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformExtensionCtor(methodDef, instr)), "This Xaml still generates a new OnPlatformExtension()");

				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.True(methodDef.Body.Instructions.Any(instr => instr.Operand as string == expected), $"Did not find instruction containing '{expected}'");
			}

			[InlineData(false)]
			[InlineData(true)]
			public void ValuesAreSet(bool useCompiledXaml)
			{
				var p = new OnPlatformOptimization(useCompiledXaml);
				Assert.Equal("ringo", p.label0.Text);
				Assert.Equal("foo", p.label1.Text);
			}

			[Fact]
			[Fact(Skip = "capability disabled for now")]
			public void OnPlatformAreSimplified(string targetFramework)
			{
				MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, out bool hasLoggedErrors, targetFramework);
				Assert.True(!hasLoggedErrors);
				Assert.True(!methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformCtor(methodDef, instr)), "This Xaml still generates a new OnPlatform()");
				Assert.True(methodDef.Body.Instructions.Any(instr => InstructionIsLdstr(methodDef.Module, instr, "Mobile, eventually ?")), $"This Xaml doesn't generate content for {targetFramework}");
				Assert.True(!methodDef.Body.Instructions.Any(instr => InstructionIsLdstr(methodDef.Module, instr, "Desktop, maybe ?")), $"This Xaml generates content not required for {targetFramework}");
			}

			bool InstructionIsOnPlatformCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
			{
				if (instruction.OpCode != OpCodes.Newobj)
					return false;
				if (instruction.Operand is not MethodReference methodRef)
					return false;
				if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(OnPlatform<View>))))
					return false;
				return true;
			}

			bool InstructionIsOnPlatformExtensionCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
			{
				if (instruction.OpCode != OpCodes.Newobj)
					return false;
				if (instruction.Operand is not MethodReference methodRef)
					return false;
				if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(OnPlatformExtension))))
					return false;
				return true;
			}

			bool InstructionIsLdstr(ModuleDefinition module, Mono.Cecil.Cil.Instruction instruction, string str)
			{
				if (instruction.OpCode != OpCodes.Ldstr)
					return false;
				return instruction.Operand as string == str;
			}
		}
	}
}