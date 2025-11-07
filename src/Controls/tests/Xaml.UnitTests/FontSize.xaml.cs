using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FontSize : ContentPage
{
	public FontSize() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void FontSizeExtensionsAreReplaced(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(FontSize), out var methodDef, out var hasLoggedErrors);
				Assert.True(!hasLoggedErrors);
				Assert.True(!methodDef.Body.Instructions.Any(instr => InstructionIsFontSizeConverterCtor(methodDef, instr)), "This Xaml still generates a new FontSizeConverter()");
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class FontSize : ContentPage
{
	public FontSize() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(FontSize));
				Assert.DoesNotContain("new global::Microsoft.Maui.Controls.FontSizeConverter()", result.GeneratedInitializeComponent());
			}
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

		[Theory]
		[Values]
		public void CorrectFontSizes(XamlInflator inflator)
		{
			var page = new FontSize(inflator);
			Assert.Equal(42, page.l42.FontSize);

#pragma warning disable CS0612 // Type or member is obsolete
			Assert.Equal(Device.GetNamedSize(NamedSize.Medium, typeof(Label)), page.lmedium.FontSize);
			Assert.Equal(Device.GetNamedSize(NamedSize.Default, typeof(Label)), page.ldefault.FontSize);
			Assert.Equal(Device.GetNamedSize(NamedSize.Default, typeof(Button)), page.bdefault.FontSize);
#pragma warning restore CS0612 // Type or member is obsolete

		}
	}
}
