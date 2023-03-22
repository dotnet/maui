using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

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
		}

		[TestFixture]
		public class Tests
		{
			[Test]
			public void OnPlatformExtensionsAreSimplified([Values("net7.0-ios", "net7.0-android")] string targetFramework)
			{
				MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, targetFramework);
				Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformExtensionCtor(methodDef, instr)), "This Xaml still generates a new OnPlatformExtension()");

				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.That(methodDef.Body.Instructions.Any(instr => instr.Operand as string == expected), $"Did not find instruction containing '{expected}'");
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ValuesAreSet(bool useCompiledXaml)
			{
				var p = new OnPlatformOptimization(useCompiledXaml);
				Assert.AreEqual("ringo", p.label0.Text);
				Assert.AreEqual("foo", p.label1.Text);
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