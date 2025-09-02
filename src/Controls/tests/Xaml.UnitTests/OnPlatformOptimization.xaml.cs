using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatformOptimization : ContentPage
{
	public OnPlatformOptimization() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void OnPlatformExtensionsAreSimplified([Values("net7.0-ios", "net7.0-android")] string targetFramework, [Values(XamlInflator.XamlC, XamlInflator.SourceGen)] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, out var hasLoggedErrors, targetFramework);
				Assert.That(!hasLoggedErrors);
				Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformExtensionCtor(methodDef, instr)), "This Xaml still generates a new OnPlatformExtension()");

				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.That(methodDef.Body.Instructions.Any(instr => instr.Operand as string == expected), $"Did not find instruction containing '{expected}'");
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatformOptimization : ContentPage
{
	public OnPlatformOptimization() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(OnPlatformOptimization), targetFramework: targetFramework);
				Assert.That(result.Diagnostics, Is.Empty);

				var generated = result.GeneratedInitializeComponent();
				Assert.That(generated, Does.Not.Contain("OnPlatformExtension"));
				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.That(generated, Does.Contain($"SetValue(global::Microsoft.Maui.Controls.Label.TextProperty, \"{expected}\");"));
			}
		}

		[Test]
		public void ValuesAreSet([Values] XamlInflator inflator)
		{
			var p = new OnPlatformOptimization(inflator);
			Assert.AreEqual("ringo", p.label0.Text);
			Assert.AreEqual("foo", p.label1.Text);
		}

		[Test]
		[Ignore("capability disabled for now")]
		public void OnPlatformAreSimplified([Values("net6.0-ios", "net6.0-android")] string targetFramework)
		{
			MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, out bool hasLoggedErrors, targetFramework);
			Assert.That(!hasLoggedErrors);
			Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformCtor(methodDef, instr)), "This Xaml still generates a new OnPlatform()");
			Assert.That(methodDef.Body.Instructions.Any(instr => InstructionIsLdstr(methodDef.Module, instr, "Mobile, eventually ?")), $"This Xaml doesn't generate content for {targetFramework}");
			Assert.That(!methodDef.Body.Instructions.Any(instr => InstructionIsLdstr(methodDef.Module, instr, "Desktop, maybe ?")), $"This Xaml generates content not required for {targetFramework}");
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