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

public partial class OnPlatformOptimization : ContentPage
{
	public OnPlatformOptimization() => InitializeComponent();

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
		[InlineData("net7.0-ios", XamlInflator.XamlC)]
		[InlineData("net7.0-android", XamlInflator.XamlC)]
		[InlineData("net7.0-ios", XamlInflator.SourceGen)]
		[InlineData("net7.0-android", XamlInflator.SourceGen)]
		public void OnPlatformExtensionsAreSimplified(string targetFramework, XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, out var hasLoggedErrors, targetFramework);
				Assert.False(hasLoggedErrors);
				Assert.False(methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformExtensionCtor(methodDef, instr)), "This Xaml still generates a new OnPlatformExtension()");

				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.True(methodDef.Body.Instructions.Any(instr => instr.Operand as string == expected), $"Did not find instruction containing '{expected}'");
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatformOptimization : ContentPage
{
	public OnPlatformOptimization() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(OnPlatformOptimization), targetFramework: targetFramework);
				Assert.Empty(result.Diagnostics);

				var generated = result.GeneratedInitializeComponent();
				Assert.DoesNotContain("OnPlatformExtension", generated);
				var expected = targetFramework.EndsWith("-ios") ? "bar" : "foo";
				Assert.Contains($"SetValue(global::Microsoft.Maui.Controls.Label.TextProperty, \"{expected}\");", generated);
			}
		}

		[Theory]
		[Values]
		public void ValuesAreSet(XamlInflator inflator)
		{
			var p = new OnPlatformOptimization(inflator);
			Assert.Equal("ringo", p.label0.Text);
			Assert.Equal("foo", p.label1.Text);
		}

		[Fact(Skip = "capability disabled for now")]
		public void OnPlatformAreSimplified()
		{
			// TODO: This test needs refactoring to support xUnit Theory pattern with multiple target frameworks
			// Original test: [Test][Ignore("capability disabled for now")]
			// public void OnPlatformAreSimplified([Values("net6.0-ios", "net6.0-android")] string targetFramework)
			var targetFramework = "net6.0-ios"; // Placeholder - needs proper parameterization
			
			MockCompiler.Compile(typeof(OnPlatformOptimization), out var methodDef, out bool hasLoggedErrors, targetFramework);
			Assert.False(hasLoggedErrors);
			Assert.False(methodDef.Body.Instructions.Any(instr => InstructionIsOnPlatformCtor(methodDef, instr)), "This Xaml still generates a new OnPlatform()");
			Assert.True(methodDef.Body.Instructions.Any(instr => InstructionIsLdstr(methodDef.Module, instr, "Mobile, eventually ?")), $"This Xaml doesn't generate content for {targetFramework}");
			Assert.False(methodDef.Body.Instructions.Any(instr => InstructionIsLdstr(methodDef.Module, instr, "Desktop, maybe ?")), $"This Xaml generates content not required for {targetFramework}");
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