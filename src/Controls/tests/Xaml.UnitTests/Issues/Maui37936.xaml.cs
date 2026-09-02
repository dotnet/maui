using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui37936 : Button
{
	public Maui37936() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Fact]
		internal void OnPlatformWithMissingTargetPlatformShouldUseDefaultInXamlC()
		{
			MockCompiler.Compile(typeof(Maui37936), out var methodDef, out var hasLoggedErrors, targetFramework: "net10.0-ios");

			Assert.False(hasLoggedErrors);
			Assert.Contains(methodDef.Body.Instructions, instruction => instruction.OpCode == OpCodes.Ldnull);
			Assert.DoesNotContain(methodDef.Body.Instructions, instruction =>
				instruction.OpCode == OpCodes.Newobj &&
				instruction.Operand is MethodReference methodReference &&
				Build.Tasks.TypeRefComparer.Default.Equals(methodReference.DeclaringType, methodDef.Module.ImportReference(typeof(ImageSource))));
			Assert.DoesNotContain(methodDef.Body.Instructions, instruction =>
				instruction.OpCode == OpCodes.Stfld &&
				instruction.Operand is FieldReference fieldReference &&
				fieldReference.Name == "transientNamescope");
		}
	}
}
