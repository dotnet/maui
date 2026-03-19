using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FontSize : ContentPage
{
	public FontSize() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void FontSizeExtensionsAreReplaced(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(FontSize), out var methodDef, out var hasLoggedErrors);
				Assert.False(hasLoggedErrors);
				Assert.False(methodDef.Body.Instructions.Any(instr => InstructionIsFontSizeConverterCtor(methodDef, instr)), "This Xaml still generates a new FontSizeConverter()");
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class FontSize : ContentPage
{
	public FontSize() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(FontSize));
				Assert.DoesNotContain("new global::Microsoft.Maui.Controls.FontSizeConverter()", result.GeneratedInitializeComponent(), StringComparison.Ordinal);
			}
		}

		static bool InstructionIsFontSizeConverterCtor(MethodDefinition methodDef, Mono.Cecil.Cil.Instruction instruction)
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
		[XamlInflatorData]
		internal void CorrectFontSizes(XamlInflator inflator)
		{
			var page = new FontSize(inflator);
			Assert.Equal(42, page.l42.FontSize);

#pragma warning disable CS0612 // Type or member is obsolete
			Assert.Equal(Device.GetNamedSize(NamedSize.Medium, page.lmedium), page.lmedium.FontSize);
			Assert.Equal(Device.GetNamedSize(NamedSize.Default, page.ldefault), page.ldefault.FontSize);
			Assert.Equal(Device.GetNamedSize(NamedSize.Default, page.bdefault), page.bdefault.FontSize);
#pragma warning restore CS0612 // Type or member is obsolete

		}
	}
}
