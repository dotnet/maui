using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Essentials;
using NUnit.Framework;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class OnPlatformOptimization : ContentPage
	{
		public OnPlatformOptimization()
		{
			InitializeComponent();
		}

		public OnPlatformOptimization(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[Test]
			public void OnPlatformExtensionsAreSimplified([Values("net6.0-ios", "net6.0-android")] string targetFramework)
			{
				MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, targetFramework);
				Assert.That(!methodDef.Body.Instructions.Any(instr=>InstructionIsOnPlatformExtensionCtor(methodDef, instr)), "This Xaml still generates a new OnPlatformExtension()");
			}

			bool InstructionIsOnPlatformExtensionCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
			{
				if (instruction.OpCode != OpCodes.Newobj)
					return false;
				if (!(instruction.Operand is MethodReference methodRef))
					return false;
				if (!Build.Tasks.TypeRefComparer.Default.Equals(methodRef.DeclaringType, methodDef.Module.ImportReference(typeof(OnPlatformExtension))))
					return false;
				return true;
			}
		}
	}
}