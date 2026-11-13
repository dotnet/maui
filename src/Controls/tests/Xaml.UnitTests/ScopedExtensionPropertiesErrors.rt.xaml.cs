using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// The ways an extension property fails to resolve. The fixtures are .rt.xaml so the build does not
// process them; the source generator is driven explicitly, which is also the only inflator supporting the
// feature. See ExtensionPropertyConventions.
public partial class ScopedExtensionPropertiesErrors : ContentPage
{
	public ScopedExtensionPropertiesErrors() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		static GeneratorDriverRunResult Generate(Type xamlType) =>
			CreateMauiCompilation()
				.AddReferences(
					MetadataReference.CreateFromFile(typeof(ScopedExtensionPropertiesErrors).Assembly.Location),
					MetadataReference.CreateFromFile(typeof(global::Controls.Xaml.UnitTests.ExternalAssembly.ExternalLabelExtensions).Assembly.Location))
				.WithAdditionalSource(
$$"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class {{xamlType.Name}} : ContentPage
{
	public {{xamlType.Name}}() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(xamlType);

		[Fact]
		public void AmbiguousAcrossContainersInScope()
		{
			// ScopedAmbiguousAExtensions declares it for IView, ScopedAmbiguousBExtensions for BindableObject
			var result = Generate(typeof(ScopedExtensionPropertiesErrors));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2021"));
		}

		[Fact]
		public void ContainerInAnUnmappedNamespaceIsNotInScope()
		{
			var result = Generate(typeof(ScopedExtensionPropertiesOutOfScope));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
		}

		[Fact]
		public void NamedContainerNeverFallsBackToTheTargetsOwnProperty()
		{
			// ExternalLabelExtensions is a real extension container, but declares no Text. Label.Text must
			// not be assigned just because the qualifier was dropped
			var result = Generate(typeof(ScopedExtensionPropertiesQualified));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2020"));
		}

		[Fact]
		public void LookalikeStaticClassIsNotAnExtensionContainer()
		{
			// the members of LookalikeExtensions have the shape of lowered accessors, but the class carries
			// no extension declaration, so it resolves like any other ordinary owner and finds nothing
			var result = Generate(typeof(ScopedExtensionPropertiesLookalike));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
		}

		[Fact]
		public void AmbiguityInsideOneContainerIsNotHiddenByALessSpecificCandidate()
		{
			// NestedAmbiguityExtensions declares it for IView and BindableObject, the fallback container for object
			var result = Generate(typeof(ScopedExtensionPropertiesNestedAmbiguity));

			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2021");
			Assert.DoesNotContain("set_NestedAmbiguous", GeneratedCode(result), StringComparison.Ordinal);
		}

		[Fact]
		public void UnassignableInstanceMemberIsNotHiddenByAnExtensionProperty()
		{
			var result = Generate(typeof(ScopedExtensionPropertiesReadOnlyMember));

			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2002");
			Assert.DoesNotContain("set_ReadOnlyClr", GeneratedCode(result), StringComparison.Ordinal);
		}

		[Fact]
		public void ContainerFromAnUnmappedAssemblyIsNotInScope()
		{
			// LeakedExtensions lives in a namespace the MAUI xmlns maps, but for another assembly
			var result = Generate(typeof(ScopedExtensionPropertiesLeaked));

			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2002");
			Assert.DoesNotContain("set_LeakedTag", GeneratedCode(result), StringComparison.Ordinal);
		}

		static string GeneratedCode(GeneratorDriverRunResult result)
			=> string.Join("\n", result.GeneratedTrees.Select(t => t.ToString()));
	}
}

public partial class ScopedExtensionPropertiesOutOfScope : ContentPage
{
	public ScopedExtensionPropertiesOutOfScope() => InitializeComponent();
}

public partial class ScopedExtensionPropertiesQualified : ContentPage
{
	public ScopedExtensionPropertiesQualified() => InitializeComponent();
}

public partial class ScopedExtensionPropertiesLookalike : ContentPage
{
	public ScopedExtensionPropertiesLookalike() => InitializeComponent();
}

public partial class ScopedExtensionPropertiesNestedAmbiguity : ContentPage
{
	public ScopedExtensionPropertiesNestedAmbiguity() => InitializeComponent();
}

public partial class ScopedExtensionPropertiesReadOnlyMember : ContentPage
{
	public ScopedExtensionPropertiesReadOnlyMember() => InitializeComponent();
}

public partial class ScopedExtensionPropertiesLeaked : ContentPage
{
	public ScopedExtensionPropertiesLeaked() => InitializeComponent();
}
