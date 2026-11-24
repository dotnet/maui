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
		static void AssertFeatureAvailable()
		{
			const string xaml =
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensionProperties">
	<Label ScopedTag="positive control" />
</ContentPage>
""";
			var result = CreateMauiCompilation()
				.AddReferences(MetadataReference.CreateFromFile(typeof(ScopedExtensionPropertiesErrors).Assembly.Location))
				.WithAdditionalSource("namespace Microsoft.Maui.Controls.Xaml.UnitTests; public partial class ScopedExtensionProperties : ContentPage { }")
				.RunMauiSourceGenerator(new AdditionalXamlFile("ScopedExtensionProperties.rt.xaml", xaml));
			Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error)
				.Select(d => $"{d.Id}: {d.GetMessage()}"));
		}

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
		public void ScopedExtensionPropertiesErrors_AmbiguousAcrossContainersInScope()
		{
			AssertFeatureAvailable();
			// ScopedAmbiguousAExtensions declares it for IView, ScopedAmbiguousBExtensions for BindableObject
			var result = Generate(typeof(ScopedExtensionPropertiesErrors));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2021"));
		}

		[Fact]
		public void ScopedExtensionPropertiesOutOfScope_ContainerInAnUnmappedNamespaceIsNotInScope()
		{
			AssertFeatureAvailable();
			var result = Generate(typeof(ScopedExtensionPropertiesOutOfScope));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
		}

		[Fact]
		public void ScopedExtensionPropertiesQualified_NamedContainerNeverFallsBackToTheTargetsOwnProperty()
		{
			AssertFeatureAvailable();
			// ExternalLabelExtensions is a real extension container, but declares no Text. Label.Text must
			// not be assigned just because the qualifier was dropped
			var result = Generate(typeof(ScopedExtensionPropertiesQualified));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2022"));
		}

		[Fact]
		public void ScopedExtensionPropertiesLookalike_LookalikeStaticClassIsNotAnExtensionContainer()
		{
			AssertFeatureAvailable();
			// the members of LookalikeExtensions have the shape of lowered accessors, but the class carries
			// no extension declaration, so it resolves like any other ordinary owner and finds nothing
			var result = Generate(typeof(ScopedExtensionPropertiesLookalike));

			Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2002"));
		}

		[Fact]
		public void ScopedExtensionPropertiesNestedAmbiguity_AmbiguityInsideOneContainerIsNotHiddenByALessSpecificCandidate()
		{
			AssertFeatureAvailable();
			// NestedAmbiguityExtensions declares it for IView and BindableObject, the fallback container for object
			var result = Generate(typeof(ScopedExtensionPropertiesNestedAmbiguity));

			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2021");
			Assert.DoesNotContain("set_NestedAmbiguous", GeneratedCode(result), StringComparison.Ordinal);
		}

		[Fact]
		public void ScopedExtensionPropertiesNestedAmbiguity_QualifiedContainerStillReportsAmbiguity()
		{
			AssertFeatureAvailable();
			const string xaml =
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:scoped="clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensions"
	x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensionPropertiesNestedAmbiguity">
	<Label scoped:NestedAmbiguityExtensions.NestedAmbiguous="ambiguous" />
</ContentPage>
""";
			var compilation = CreateMauiCompilation()
				.AddReferences(MetadataReference.CreateFromFile(typeof(ScopedExtensionPropertiesErrors).Assembly.Location))
				.WithAdditionalSource("namespace Microsoft.Maui.Controls.Xaml.UnitTests; public partial class ScopedExtensionPropertiesNestedAmbiguity : ContentPage { }");
			var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("ScopedExtensionPropertiesNestedAmbiguity.rt.xaml", xaml));
			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2021");
			Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2022");
		}

		[Fact]
		public void ScopedExtensionPropertiesErrors_UserDefinedReceiverConversionIsNotApplicable()
		{
			AssertFeatureAvailable();
			const string xaml =
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.ScopedExtensionPropertiesErrors">
	<ConvertibleExtensionView UserConversionTag="not an applicable receiver" />
</ContentPage>
""";
			var compilation = CreateMauiCompilation()
				.AddReferences(MetadataReference.CreateFromFile(typeof(ScopedExtensionPropertiesErrors).Assembly.Location))
				.WithAdditionalSource("namespace Microsoft.Maui.Controls.Xaml.UnitTests; public partial class ScopedExtensionPropertiesErrors : ContentPage { }");
			var result = compilation.RunMauiSourceGenerator(new AdditionalXamlFile("ScopedExtensionPropertiesErrors.rt.xaml", xaml));
			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2002");
		}

		[Fact]
		public void ScopedExtensionPropertiesReadOnlyMember_UnassignableInstanceMemberIsNotHiddenByAnExtensionProperty()
		{
			AssertFeatureAvailable();
			var result = Generate(typeof(ScopedExtensionPropertiesReadOnlyMember));

			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIX2002");
			Assert.DoesNotContain("set_ReadOnlyClr", GeneratedCode(result), StringComparison.Ordinal);
		}

		[Fact]
		public void ScopedExtensionPropertiesLeaked_ContainerFromAnUnmappedAssemblyIsNotInScope()
		{
			AssertFeatureAvailable();
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
