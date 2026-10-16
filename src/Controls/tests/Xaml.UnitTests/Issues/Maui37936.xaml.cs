using System;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[AcceptEmptyServiceProvider]
public class Maui37936MarkupExtension : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider) => this;
}

public partial class Maui37936 : Button
{
	public Maui37936() => InitializeComponent();

	public Maui37936MarkupExtension DefaultMarkupExtension { get; set; }

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Fact]
		internal void OnPlatformWithMissingTargetPlatformShouldUseDefaultInXamlC()
		{
			MockCompiler.Compile(typeof(Maui37936), out var methodDef, out var hasLoggedErrors, targetFramework: "net10.0-ios");

			Assert.False(hasLoggedErrors);
			AssertNullStoredInLocal(methodDef, typeof(ImageSource));
			AssertNullStoredInLocal(methodDef, typeof(Maui37936MarkupExtension));
			Assert.DoesNotContain(methodDef.Body.Instructions, instruction =>
				instruction.OpCode == OpCodes.Newobj &&
				instruction.Operand is MethodReference methodReference &&
				Build.Tasks.TypeRefComparer.Default.Equals(methodReference.DeclaringType, methodDef.Module.ImportReference(typeof(ImageSource))));
			Assert.DoesNotContain(methodDef.Body.Instructions, instruction =>
				instruction.OpCode == OpCodes.Stfld &&
				instruction.Operand is FieldReference fieldReference &&
				fieldReference.Name == "transientNamescope");
			Assert.DoesNotContain(methodDef.Body.Instructions, instruction =>
				instruction.OpCode == OpCodes.Callvirt &&
				instruction.Operand is MethodReference methodReference &&
				methodReference.Name == nameof(IMarkupExtension.ProvideValue));
		}

		[Fact]
		internal void OnPlatformWithMissingTargetPlatformShouldNotInvokeProvideValueInSourceGen()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using System;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[global::Microsoft.Maui.Controls.Xaml.AcceptEmptyServiceProvider]
public class Maui37936MarkupExtension : global::Microsoft.Maui.Controls.Xaml.IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider) => this;
}

[global::Microsoft.Maui.Controls.Xaml.XamlProcessing(global::Microsoft.Maui.Controls.Xaml.XamlInflator.Runtime, true)]
public partial class Maui37936 : global::Microsoft.Maui.Controls.Button
{
	public Maui37936MarkupExtension DefaultMarkupExtension { get; set; }

	public Maui37936() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(Maui37936), targetFramework: "net10.0-ios");

			Assert.Empty(result.Diagnostics);
			Assert.DoesNotContain(".ProvideValue(", result.GeneratedInitializeComponent(), StringComparison.Ordinal);
		}

		static void AssertNullStoredInLocal(MethodDefinition methodDef, Type expectedType)
		{
			var expectedTypeReference = methodDef.Module.ImportReference(expectedType);
			Assert.Contains(methodDef.Body.Instructions, instruction =>
				instruction.OpCode == OpCodes.Ldnull &&
				instruction.Next?.OpCode == OpCodes.Stloc &&
				instruction.Next.Operand is VariableDefinition variable &&
				Build.Tasks.TypeRefComparer.Default.Equals(variable.VariableType, expectedTypeReference));
		}
	}
}
