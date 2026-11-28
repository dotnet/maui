using System;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class GeneratesBindablePropertyTests : SourceGenXamlInitializeComponentTestBase
{
	const string MarkerAndLibraryAttributes =
"""
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Xaml
{
	[AttributeUsage(AttributeTargets.Class)]
	internal sealed class GeneratesBindablePropertyAttribute : Attribute
	{
		public bool IsAttached { get; set; }
		public int NameConstructorArgument { get; set; }
		public string? NameNamedArgument { get; set; }
		public int ValueTypeGenericArgument { get; set; }
		public int ValueTypeConstructorArgument { get; set; }
		public string? FieldNameFormat { get; set; }
		public string? GetterNameFormat { get; set; }
		public string? SetterNameFormat { get; set; }
		public bool IsReadOnly { get; set; }
		public string? IsReadOnlyNamedArgument { get; set; }
		public string? FieldAccessibilityNamedArgument { get; set; }
		public string? GetterAccessibilityNamedArgument { get; set; }
		public string? SetterAccessibilityNamedArgument { get; set; }
		public string[]? AccessibilityValues { get; set; }
	}
}

namespace Acme
{
	public enum Speed { Slow, Fast }

	public enum Access { Public, Internal, ProtectedInternal, Protected, PrivateProtected, Private, None }

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0)]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Constructor, AllowMultiple = true)]
	public sealed class AttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
	}

	[GeneratesBindableProperty]
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class BindableAttribute : Attribute
	{
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0)]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class UntypedAttachedAttribute(string name) : Attribute
	{
		public string Name { get; } = name;
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class UnmarkedAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
	}

	[GeneratesBindableProperty(IsAttached = true, NameNamedArgument = nameof(PropertyName), ValueTypeConstructorArgument = 0)]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class TypeofAttachedAttribute(Type valueType) : Attribute
	{
		public Type ValueType { get; } = valueType;
		public string? PropertyName { get; init; }
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, FieldNameFormat = "{name}Key", GetterNameFormat = "Read{name}", SetterNameFormat = "Write{name}")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class FormattedAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, IsReadOnly = true)]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class ReadOnlyAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, IsReadOnlyNamedArgument = nameof(IsReadOnly))]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class PerUseReadOnlyAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
		public bool IsReadOnly { get; init; }
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, FieldAccessibilityNamedArgument = nameof(FieldAccess), GetterAccessibilityNamedArgument = nameof(GetterAccess), SetterAccessibilityNamedArgument = nameof(SetterAccess),
		AccessibilityValues = ["public", "internal", "protected internal", "protected", "private protected", "private", ""])]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class AccessAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
		public Access FieldAccess { get; init; }
		public Access GetterAccess { get; init; }
		public Access SetterAccess { get; init; }
	}

	public enum Visibility { Everywhere, Assembly, Hidden }

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, FieldAccessibilityNamedArgument = nameof(FieldVisibility), AccessibilityValues = ["public", "internal", ""])]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class VisibilityAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
		public Visibility FieldVisibility { get; init; }
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, FieldAccessibilityNamedArgument = nameof(FieldVisibility), AccessibilityValues = ["public"])]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class UnmappedVisibilityAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
		public Visibility FieldVisibility { get; init; }
	}

	[GeneratesBindableProperty(IsAttached = true, NameConstructorArgument = 0, ValueTypeGenericArgument = 0, FieldAccessibilityNamedArgument = nameof(FieldKeywords))]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public sealed class KeywordAttachedAttribute<T>(string name) : Attribute where T : notnull
	{
		public string Name { get; } = name;
		public string FieldKeywords { get; init; } = "public";
	}
}
""";

	const string SiblingGeneratedMembers =
"""
#nullable enable
namespace Test
{
	public static partial class Emphasis
	{
		public static readonly Microsoft.Maui.Controls.BindableProperty LevelProperty = Microsoft.Maui.Controls.BindableProperty.CreateAttached("Level", typeof(int), typeof(Emphasis), default(int));
		public static int GetLevel(Microsoft.Maui.Controls.BindableObject bindable) => (int)bindable.GetValue(LevelProperty);
		public static void SetLevel(Microsoft.Maui.Controls.BindableObject bindable, int value) => bindable.SetValue(LevelProperty, value);
		public static readonly Microsoft.Maui.Controls.BindableProperty SpeedProperty = Microsoft.Maui.Controls.BindableProperty.CreateAttached("Speed", typeof(Acme.Speed), typeof(Emphasis), default(Acme.Speed));
		public static Acme.Speed GetSpeed(Microsoft.Maui.Controls.BindableObject bindable) => (Acme.Speed)bindable.GetValue(SpeedProperty);
		public static void SetSpeed(Microsoft.Maui.Controls.BindableObject bindable, Acme.Speed value) => bindable.SetValue(SpeedProperty, value);
	}

	public partial class Highlight
	{
		public static readonly Microsoft.Maui.Controls.BindableProperty WeightProperty = Microsoft.Maui.Controls.BindableProperty.CreateAttached("Weight", typeof(double), typeof(Highlight), default(double));
		public static double GetWeight(Microsoft.Maui.Controls.BindableObject bindable) => (double)bindable.GetValue(WeightProperty);
		public static void SetWeight(Microsoft.Maui.Controls.BindableObject bindable, double value) => bindable.SetValue(WeightProperty, value);
	}

	public partial class Badge
	{
		public static readonly Microsoft.Maui.Controls.BindableProperty CaptionProperty = Microsoft.Maui.Controls.BindableProperty.Create("Caption", typeof(string), typeof(Badge), default(string));
		public partial string? Caption
		{
			get => (string?)GetValue(CaptionProperty);
			set => SetValue(CaptionProperty, value);
		}
	}
}
""";

	const string Hosts =
"""
namespace Test
{
	[Acme.Attached<int>("Level")]
	[Acme.Attached<Acme.Speed>("Speed")]
	public static partial class Emphasis;

	public partial class Highlight : BindableObject
	{
		[Acme.Attached<double>("Weight")]
		public Highlight() { }
	}

	public partial class Badge : ContentView
	{
		[Acme.Bindable]
		public partial string? Caption { get; set; }
	}

	public class ViewModel
	{
		public int Number { get; set; }
		public string? Name { get; set; }
		public bool Flag { get; set; }
	}

	public partial class TestPage : ContentPage
	{
		public TestPage() => InitializeComponent();
	}
}
""";

	const string PageStart =
"""
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Test"
             x:Class="Test.TestPage"
             x:DataType="local:ViewModel">
""";

	static string Page(string content) => $"{PageStart}{content}\n</ContentPage>";

	(GeneratorDriverRunResult Result, string Output) RunWithLibraryMarkedAttributes(string xamlContent, string hosts = Hosts)
	{
		var (result, output) = RunGenerator(Page(xamlContent), $"{MarkerAndLibraryAttributes}\n{hosts}", assertNoCompilationErrors: false);
		Assert.True(output is not null, string.Join("\n", result.Diagnostics));
		return (result, output!);
	}

	[Fact]
	public void AttachedProperty_AttributeSyntax_ConvertsAtCompileTimeAndCompilesWithRealMembers()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="3" local:Emphasis.Speed="Fast" />
    <Label local:Highlight.Weight="2.5" />
</VerticalStackLayout>
""");

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", output, StringComparison.Ordinal);
		Assert.Contains("SetValue(global::Test.Emphasis.SpeedProperty, global::Acme.Speed.Fast);", output, StringComparison.Ordinal);
		Assert.Contains("SetValue(global::Test.Highlight.WeightProperty, 2.5D);", output, StringComparison.Ordinal);
		AssertGeneratedCodeCompilesWithSiblingSource(result);
	}

	[Fact]
	public void AttachedProperty_BindingPropertyElementAndMarkupExtension_AreResolved()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="{Binding Number}" />
    <Label>
        <local:Emphasis.Level>5</local:Emphasis.Level>
    </Label>
    <Label local:Emphasis.Level="{OnPlatform Default=8}" />
</VerticalStackLayout>
""");

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetBinding(global::Test.Emphasis.LevelProperty,", output, StringComparison.Ordinal);
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 5);", output, StringComparison.Ordinal);
		AssertGeneratedCodeCompilesWithSiblingSource(result);
	}

	[Fact]
	public void AttachedProperty_StyleTriggerAndVisualStateSetters_AreResolved()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<ContentPage.Resources>
    <Style x:Key="levelStyle" TargetType="Label">
        <Setter Property="local:Emphasis.Level" Value="4" />
        <Setter Property="local:Emphasis.Speed" Value="Fast" />
    </Style>
</ContentPage.Resources>
<VerticalStackLayout>
    <Label Style="{StaticResource levelStyle}" />
    <Label>
        <Label.Triggers>
            <DataTrigger TargetType="Label" Binding="{Binding Flag}" Value="True">
                <Setter Property="local:Emphasis.Level" Value="6" />
            </DataTrigger>
        </Label.Triggers>
        <VisualStateManager.VisualStateGroups>
            <VisualStateGroup x:Name="states">
                <VisualState x:Name="Active">
                    <VisualState.Setters>
                        <Setter Property="local:Emphasis.Level" Value="7" />
                    </VisualState.Setters>
                </VisualState>
            </VisualStateGroup>
        </VisualStateManager.VisualStateGroups>
    </Label>
</VerticalStackLayout>
""");

		Assert.Empty(result.Diagnostics);
		Assert.Contains("Property = global::Test.Emphasis.LevelProperty, Value = 4", output, StringComparison.Ordinal);
		Assert.Contains("Property = global::Test.Emphasis.SpeedProperty, Value = global::Acme.Speed.Fast", output, StringComparison.Ordinal);
		Assert.Contains("Property = global::Test.Emphasis.LevelProperty, Value = 6", output, StringComparison.Ordinal);
		Assert.Contains("Property = global::Test.Emphasis.LevelProperty, Value = 7", output, StringComparison.Ordinal);
		AssertGeneratedCodeCompilesWithSiblingSource(result);
	}

	[Fact]
	public void RegularProperty_BindingOnMarkedPropertyAttribute_IsResolved()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <local:Badge Caption="{Binding Name}" />
    <local:Badge Caption="hello" />
    <local:Badge Caption="{DynamicResource CaptionText}" />
</VerticalStackLayout>
""");

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetBinding(global::Test.Badge.CaptionProperty,", output, StringComparison.Ordinal);
		Assert.Contains("SetDynamicResource(global::Test.Badge.CaptionProperty,", output, StringComparison.Ordinal);
		AssertGeneratedCodeCompilesWithSiblingSource(result);
	}

	[Fact]
	public void MisspelledAttachedProperty_StillReportsMemberResolutionAtTheXamlLine()
	{
		var (result, _) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Levl="3" />
</VerticalStackLayout>
""");

		var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2002");
		Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
		Assert.Equal(6, diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1);
	}

	[Fact]
	public void MisspelledBindingOnMarkedProperty_StillReportsMemberResolution()
	{
		var (result, _) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <local:Badge Captoin="{Binding Name}" />
</VerticalStackLayout>
""");

		Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2002");
	}

	[Fact]
	public void AttributeWithoutMarker_IsNotTrusted()
	{
		var hosts = Hosts.Replace("[Acme.Attached<int>(\"Level\")]", "[Acme.UnmarkedAttached<int>(\"Level\")]", StringComparison.Ordinal);

		var (result, _) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="3" />
</VerticalStackLayout>
""", hosts);

		Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2002");
	}

	[Fact]
	public void MarkedAttributeWithoutValueType_ResolvesBindingsAndDoesNotCrashOnLiterals()
	{
		var hosts = Hosts.Replace("[Acme.Attached<int>(\"Level\")]", "[Acme.UntypedAttached(\"Level\")]", StringComparison.Ordinal);

		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="{Binding Number}" />
</VerticalStackLayout>
""", hosts);

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetBinding(global::Test.Emphasis.LevelProperty,", output, StringComparison.Ordinal);
	}

	[Fact]
	public void MemberAlreadyDeclaredInSource_IsNotDeclaredTwice()
	{
		var hosts = Hosts.Replace(
			"public static partial class Emphasis;",
			"""
			public static partial class Emphasis
			{
				public static readonly BindableProperty LevelProperty = BindableProperty.CreateAttached("Level", typeof(int), typeof(Emphasis), 0);
				public static int GetLevel(BindableObject bindable) => (int)bindable.GetValue(LevelProperty);
			}
			""", StringComparison.Ordinal);

		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="3" />
</VerticalStackLayout>
""", hosts);

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", output, StringComparison.Ordinal);
	}

	const string PageAndViewModelTypes =
"""
	public class ViewModel
	{
		public int Number { get; set; }
		public string? Name { get; set; }
		public bool Flag { get; set; }
	}

	public partial class TestPage : ContentPage
	{
		public TestPage() => InitializeComponent();
	}
""";

	static string HostsWith(string hostTypes) => $"namespace Test\n{{\n{hostTypes}\n{PageAndViewModelTypes}\n}}";

	static string Errors(GeneratorDriverRunResult result) => string.Join("\n", result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));

	[Fact]
	public void AttachedProperty_NameFromNamedArgumentAndTypeFromTypeofArgument_IsResolved()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Counter.Count="3" />
    <Label local:Counter.Count="{Binding Number}" />
</VerticalStackLayout>
""", HostsWith("""
	[Acme.TypeofAttached(typeof(int), PropertyName = "Count")]
	public static partial class Counter;
"""));

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetValue(global::Test.Counter.CountProperty, 3);", output, StringComparison.Ordinal);
		Assert.Contains("SetBinding(global::Test.Counter.CountProperty,", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AttachedProperty_NameNotWhereTheContractSays_DeclaresNothing()
	{
		var (result, _) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Counter.Count="3" />
</VerticalStackLayout>
""", HostsWith("""
	[Acme.TypeofAttached(typeof(int))]
	public static partial class Counter;
"""));

		Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2002");
	}

	[Fact]
	public void MemberNameFormats_AreTakenFromTheContract()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label BindingContext="{x:Static local:Emphasis.LevelKey}" />
    <Label local:Emphasis.Level="3" />
</VerticalStackLayout>
""", HostsWith("""
	[Acme.FormattedAttached<int>("Level")]
	public static partial class Emphasis;
"""));

		Assert.Contains("global::Test.Emphasis.LevelKey", output, StringComparison.Ordinal);
		Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2002");
	}

	[Fact]
	public void ReadOnlyAttachedProperty_DeclaresTheSameMembersAsAHandWrittenGetterOnlyHost()
	{
		const string xaml =
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="{Binding Number, Mode=OneWayToSource}" />
</VerticalStackLayout>
""";

		var (marked, markedOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith("""
	[Acme.ReadOnlyAttached<int>("Level")]
	public static partial class Emphasis;
"""));

		var (handWritten, handWrittenOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith("""
	public static partial class Emphasis
	{
		public static readonly BindableProperty LevelProperty = BindableProperty.CreateAttached("Level", typeof(int), typeof(Emphasis), 0);
		public static int GetLevel(BindableObject bindable) => (int)bindable.GetValue(LevelProperty);
	}
"""));

		Assert.Equal(handWritten.Diagnostics.Select(static d => d.Id), marked.Diagnostics.Select(static d => d.Id));
		Assert.Equal(handWrittenOutput, markedOutput);
	}

	[Fact]
	public void ReadOnlyAttachedProperty_FromNamedArgument_AppliesPerUse()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="3" />
    <Label local:Emphasis.Fixed="{Binding Number, Mode=OneWay}" />
</VerticalStackLayout>
""", HostsWith("""
	[Acme.PerUseReadOnlyAttached<int>("Level")]
	[Acme.PerUseReadOnlyAttached<int>("Fixed", IsReadOnly = true)]
	public static partial class Emphasis;
"""));

		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", output, StringComparison.Ordinal);
		Assert.DoesNotContain("MAUIX2002", Errors(result), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("Public", "Public", "Public")]
	[InlineData("Internal", "Public", "Public")]
	[InlineData("Private", "Public", "Public")]
	[InlineData("Public", "Internal", "Public")]
	[InlineData("Public", "ProtectedInternal", "Public")]
	[InlineData("Public", "None", "Public")]
	[InlineData("Public", "Public", "Private")]
	[InlineData("None", "Public", "Public")]
	public void MemberAccessibility_BehavesLikeAHandWrittenHostWithTheSameAccessibility(string field, string getter, string setter)
	{
		const string xaml =
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="3" />
</VerticalStackLayout>
""";

		var (marked, markedOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith($$"""
	[Acme.AccessAttached<int>("Level", FieldAccess = Acme.Access.{{field}}, GetterAccess = Acme.Access.{{getter}}, SetterAccess = Acme.Access.{{setter}})]
	public partial class Emphasis;
"""));

		var (handWritten, handWrittenOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith($$"""
	public partial class Emphasis
	{
		{{Declaration(field, "static readonly BindableProperty LevelProperty = BindableProperty.CreateAttached(\"Level\", typeof(int), typeof(Emphasis), 0);")}}
		{{Declaration(getter, "static int GetLevel(BindableObject bindable) => (int)bindable.GetValue(LevelProperty);")}}
		{{Declaration(setter, "static void SetLevel(BindableObject bindable, int value) => bindable.SetValue(LevelProperty, value);")}}
	}
"""));

		Assert.Equal(handWritten.Diagnostics.Select(static d => d.Id), marked.Diagnostics.Select(static d => d.Id));
		Assert.Equal(handWrittenOutput, markedOutput);

		static string Declaration(string access, string member) => access switch
		{
			"Internal" => $"internal {member}",
			"ProtectedInternal" => $"protected internal {member}",
			"Private" => $"private {member}",
			"None" => string.Empty,
			_ => $"public {member}",
		};
	}

	[Fact]
	public void PublicHostOfInternalMembers_IsNotUsableFromXamlJustLikeAHandWrittenOne()
	{
		var (result, _) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <Label local:Emphasis.Level="3" />
</VerticalStackLayout>
""", HostsWith("""
	[Acme.AccessAttached<int>("Level", GetterAccess = Acme.Access.Internal)]
	public static partial class Emphasis;
"""));

		Assert.Single(result.Diagnostics, d => d.Id == "MAUIX2002");
	}

	[Fact]
	public void GenericHost_IsDeclaredWithItsTypeParameters()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
"""
<VerticalStackLayout>
    <local:GenericBadge x:TypeArguments="x:String" Caption="{Binding Name}" />
</VerticalStackLayout>
""", HostsWith("""
	public partial class GenericBadge<T> : ContentView
	{
		[Acme.Bindable]
		public partial string? Caption { get; set; }
	}
"""));

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetBinding(global::Test.GenericBadge<string>.CaptionProperty,", output, StringComparison.Ordinal);
	}

	[Fact]
	public void NestedHosts_AreDeclaredInsideTheirEnclosingTypes()
	{
		var (result, _) = RunWithLibraryMarkedAttributes(
			"<Label Text=\"plain\" />",
			HostsWith("""
	public partial class Outer
	{
		public partial class NestedBadge : ContentView
		{
			[Acme.Bindable]
			public partial string? Caption { get; set; }
		}

		[Acme.Attached<int>("Level")]
		public static partial class NestedHost;
	}
"""));

		var stubs = ((GeneratedBindablePropertyDeclarations.StubDeclarations)result.Results.Single().TrackedSteps[TrackingNames.GeneratedBindablePropertyStubsProvider].Single().Outputs.Single().Value).Stubs.ToArray();

		Assert.Equal(2, stubs.Length);
		Assert.Contains(stubs, static stub => stub.Contains("namespace Test { partial class Outer { partial class NestedBadge { public static readonly global::Microsoft.Maui.Controls.BindableProperty CaptionProperty", StringComparison.Ordinal));
		Assert.Contains(stubs, static stub => stub.Contains("namespace Test { partial class Outer { partial class NestedHost { public static readonly global::Microsoft.Maui.Controls.BindableProperty LevelProperty", StringComparison.Ordinal));
		Assert.All(stubs, static stub => Assert.Empty(CSharpSyntaxTree.ParseText(stub).GetDiagnostics()));
	}

	record ReferenceXamlFile(string Path, string Content)
		: AdditionalFile(Text: ToAdditionalText(Path, Content), Kind: "Xaml", RelativePath: Path, TargetPath: null, ManifestResourceName: null, TargetFramework: null, NoWarn: null);

	[Fact]
	public void MarkedAttributeFromReferencedAssembly_IsResolved()
	{
		var libraryCompilation = CreateMauiCompilation("AcmeLibrary").AddSyntaxTrees(CSharpSyntaxTree.ParseText(MarkerAndLibraryAttributes));
		using var stream = new System.IO.MemoryStream();
		Assert.True(libraryCompilation.Emit(stream).Success);

		var userCode = HostsWith("""
	[Acme.Attached<int>("Level")]
	public static partial class Emphasis;

	public partial class Badge : ContentView
	{
		[Acme.Bindable]
		public partial string? Caption { get; set; }
	}
""");
		var compilation = CreateMauiCompilation()
			.AddReferences(MetadataReference.CreateFromImage(stream.ToArray()))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText(userCode));
		var xamlFile = new ReferenceXamlFile("Test.xaml", Page("<VerticalStackLayout><Label local:Emphasis.Level=\"3\" /><local:Badge Caption=\"{Binding Name}\" /></VerticalStackLayout>"));

		var result = RunGenerator<XamlGenerator>(compilation, xamlFile, assertNoCompilationErrors: false);

		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIX2002");
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", result.Results.Single().GeneratedSources.Single(source => source.HintName.EndsWith(".xsg.cs", StringComparison.Ordinal)).SourceText.ToString(), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("[Acme.AttachedAttribute<int>(\"Level\")]")]
	[InlineData("[global::Acme.Attached<int>(\"Level\")]")]
	public void MarkedAttribute_IsRecognizedWhateverWayItIsQualified(string attribute)
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
			"<Label local:Emphasis.Level=\"3\" />",
			HostsWith($"\t{attribute}\n\tpublic static partial class Emphasis;"));

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", output, StringComparison.Ordinal);
	}

	[Fact]
	public void MarkedAttribute_IsRecognizedThroughAUsingAlias()
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
			"<Label local:Emphasis.Level=\"3\" />",
			HostsWith("\tusing Rank = Acme.AttachedAttribute<int>;\n\t[Rank(\"Level\")]\n\tpublic static partial class Emphasis;"));

		Assert.Empty(result.Diagnostics);
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", output, StringComparison.Ordinal);
	}

	[Fact]
	public void AccessibilityEnumOfAnyVocabulary_IsMappedByTheLibrary()
	{
		const string xaml = "<VerticalStackLayout><Label local:Emphasis.Level=\"3\" /></VerticalStackLayout>";

		var (marked, markedOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith("""
	[Acme.VisibilityAttached<int>("Level", FieldVisibility = Acme.Visibility.Assembly)]
	public static partial class Emphasis;
"""));

		var (handWritten, handWrittenOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith("""
	public static partial class Emphasis
	{
		internal static readonly BindableProperty LevelProperty = BindableProperty.CreateAttached("Level", typeof(int), typeof(Emphasis), 0);
		public static int GetLevel(BindableObject bindable) => (int)bindable.GetValue(LevelProperty);
		public static void SetLevel(BindableObject bindable, int value) => bindable.SetValue(LevelProperty, value);
	}
"""));

		Assert.Equal(handWritten.Diagnostics.Select(static d => d.Id), marked.Diagnostics.Select(static d => d.Id));
		Assert.Equal(handWrittenOutput, markedOutput);
		Assert.DoesNotContain(marked.Diagnostics, static d => d.Id == "MAUIX2026");
	}

	[Fact]
	public void AccessibilityValueTheLibraryDidNotMap_IsReportedAtTheAttribute()
	{
		var attributeLine = "\t[Acme.UnmappedVisibilityAttached<int>(\"Level\", FieldVisibility = Acme.Visibility.Hidden)]";
		var hosts = HostsWith($"{attributeLine}\n\tpublic static partial class Emphasis;");
		var xamlFile = new AdditionalXamlFile("Test.xaml", Page("<VerticalStackLayout><Label local:Emphasis.Level=\"3\" /></VerticalStackLayout>"));
		var compilation = CreateMauiCompilation().AddSyntaxTrees(
			CSharpSyntaxTree.ParseText(MarkerAndLibraryAttributes, path: "Library.cs"),
			CSharpSyntaxTree.ParseText(hosts, path: "UserCode.cs"));

		var result = RunGenerator<XamlGenerator>(compilation, xamlFile, assertNoCompilationErrors: false);

		var warning = Assert.Single(result.Diagnostics, static d => d.Id == "MAUIX2026");
		Assert.Equal(DiagnosticSeverity.Warning, warning.Severity);
		Assert.Contains("FieldVisibility", warning.GetMessage(), StringComparison.Ordinal);
		Assert.Contains("UnmappedVisibilityAttachedAttribute", warning.GetMessage(), StringComparison.Ordinal);

		var location = warning.Location.GetLineSpan();
		Assert.Equal("UserCode.cs", location.Path);
		var expectedLine = Array.FindIndex(hosts.Split('\n'), static line => line.Contains("UnmappedVisibilityAttached<int>", StringComparison.Ordinal));
		Assert.Equal(expectedLine, location.StartLinePosition.Line);
	}

	[Theory]
	[InlineData("internal", true)]
	[InlineData("protected internal", true)]
	[InlineData("internal protected", true)]
	[InlineData("Internal", false)]
	[InlineData("friend", false)]
	public void AccessibilityGivenAsAString_IsReadAsCSharpKeywords(string keywords, bool isRecognized)
	{
		var (result, _) = RunWithLibraryMarkedAttributes(
			"<Label Text=\"plain\" />",
			HostsWith($$"""
	[Acme.KeywordAttached<int>("Level", FieldKeywords = "{{keywords}}")]
	public static partial class Emphasis;
"""));

		Assert.Equal(isRecognized ? 0 : 1, result.Diagnostics.Count(static d => d.Id == "MAUIX2026"));
	}

	[Fact]
	public void NullableValueTypeOfAHandWrittenHost_BehavesLikeTheNonNullableStub()
	{
		const string xaml =
"""
<VerticalStackLayout>
    <Label local:Emphasis.Depth="{x:Null}" />
    <Label local:Emphasis.Depth="3" />
    <Label local:Emphasis.Depth="{Binding Number}" />
    <Label>
        <Label.Style><Style TargetType="Label"><Setter Property="local:Emphasis.Depth" Value="{x:Null}" /></Style></Label.Style>
    </Label>
    <Label>
        <local:Emphasis.Depth><x:Null /></local:Emphasis.Depth>
    </Label>
</VerticalStackLayout>
""";

		var (marked, markedOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith("""
	[Acme.Attached<int>("Depth")]
	public static partial class Emphasis;
"""));

		var (handWritten, handWrittenOutput) = RunWithLibraryMarkedAttributes(xaml, HostsWith("""
	public static partial class Emphasis
	{
		public static readonly BindableProperty DepthProperty = BindableProperty.CreateAttached("Depth", typeof(int?), typeof(Emphasis), null);
		public static int? GetDepth(BindableObject bindable) => (int?)bindable.GetValue(DepthProperty);
		public static void SetDepth(BindableObject bindable, int? value) => bindable.SetValue(DepthProperty, value);
	}
"""));

		Assert.Equal(handWritten.Diagnostics.Select(static d => d.Id), marked.Diagnostics.Select(static d => d.Id));
		Assert.Equal(handWrittenOutput, markedOutput);
	}

	[Theory]
	[InlineData("public partial struct Emphasis;")]
	[InlineData("public partial record Emphasis;")]
	public void HostKinds_OtherThanClasses_AreDeclaredWithTheirOwnKeywords(string declaration)
	{
		var (result, output) = RunWithLibraryMarkedAttributes(
			"<Label local:Emphasis.Level=\"3\" />",
			HostsWith($"\t[Acme.Attached<int>(\"Level\")]\n\t{declaration}"));

		Assert.DoesNotContain(result.Diagnostics, static d => d.Id == "MAUIX2002");
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", output, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("public partial record struct Emphasis;")]
	[InlineData("public partial record Emphasis { public Emphasis(int unused) { } }")]
	public void RecordHosts_AreDeclaredAsRecords(string declaration)
	{
		var attribute = declaration.Contains("(int unused)", StringComparison.Ordinal)
			? declaration.Replace("public Emphasis(int unused)", "[Acme.Attached<int>(\"Level\")] public Emphasis(int unused)", StringComparison.Ordinal)
			: $"[Acme.Attached<int>(\"Level\")] {declaration}";

		var (result, _) = RunWithLibraryMarkedAttributes("<Label Text=\"plain\" />", HostsWith($"\t{attribute}"));

		var stubs = ((GeneratedBindablePropertyDeclarations.StubDeclarations)result.Results.Single().TrackedSteps[TrackingNames.GeneratedBindablePropertyStubsProvider].Single().Outputs.Single().Value).Stubs.ToArray();
		var stub = Assert.Single(stubs);
		Assert.Empty(CSharpSyntaxTree.ParseText(stub).GetDiagnostics());
		Assert.Contains(declaration.Contains("record struct", StringComparison.Ordinal) ? "partial record struct Emphasis" : "partial record Emphasis", stub, StringComparison.Ordinal);
	}

	[Fact]
	public void MarkedAttribute_IsRecognizedThroughAGlobalUsingAliasInAnotherFile()
	{
		var xamlFile = new AdditionalXamlFile("Test.xaml", Page("<Label local:Emphasis.Level=\"3\" />"));
		var compilation = CreateMauiCompilation().AddSyntaxTrees(
			CSharpSyntaxTree.ParseText("global using Rank = Acme.AttachedAttribute<int>;", path: "GlobalUsings.cs"),
			CSharpSyntaxTree.ParseText(MarkerAndLibraryAttributes, path: "Library.cs"),
			CSharpSyntaxTree.ParseText(HostsWith("\t[Rank(\"Level\")]\n\tpublic static partial class Emphasis;"), path: "UserCode.cs"));

		var result = RunGenerator<XamlGenerator>(compilation, xamlFile, assertNoCompilationErrors: false);

		Assert.DoesNotContain(result.Diagnostics, static d => d.Id == "MAUIX2002");
		Assert.Contains("SetValue(global::Test.Emphasis.LevelProperty, 3);", result.Results.Single().GeneratedSources.Single(source => source.HintName.EndsWith(".xsg.cs", StringComparison.Ordinal)).SourceText.ToString(), StringComparison.Ordinal);
	}

	[Fact]
	public void NoMarkedAttribute_LeavesTheGeneratedOutputUnchanged()
	{
		var xaml = Page("<Label Text=\"{Binding Name}\" />");
		var hostsWithoutAttributes = HostsWith("\tpublic static partial class Emphasis;");
		var withUnmarkedAttribute = HostsWith("\t[Acme.UnmarkedAttached<int>(\"Level\")]\n\tpublic static partial class Emphasis;");

		var (_, withoutAttributes) = RunGenerator(xaml, $"{MarkerAndLibraryAttributes}\n{hostsWithoutAttributes}", assertNoCompilationErrors: false);
		var (_, withUnmarked) = RunGenerator(xaml, $"{MarkerAndLibraryAttributes}\n{withUnmarkedAttribute}", assertNoCompilationErrors: false);

		Assert.Equal(withoutAttributes, withUnmarked);
	}

	[Fact]
	public void EditingXaml_LeavesTheStubsUnchanged()
	{
		var xamlFile = new AdditionalXamlFile("Test.xaml", Page("<Label local:Emphasis.Level=\"3\" />"));
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts}", path: "UserCode.cs"));

		var (_, second) = RunGeneratorWithChanges<XamlGenerator>(compilation, (driver, currentCompilation) =>
		{
			var edited = new AdditionalXamlFile("Test.xaml", Page("<Label local:Emphasis.Level=\"4\" />"));
			return (driver.ReplaceAdditionalText(xamlFile.Text, edited.Text), currentCompilation);
		}, xamlFile);

		AssertStubsAreUnchanged(second);
	}

	[Fact]
	public void EditingUnrelatedCSharp_LeavesTheStubsUnchanged()
	{
		var xamlFile = new AdditionalXamlFile("Test.xaml", Page("<Label local:Emphasis.Level=\"3\" />"));
		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts}", path: "UserCode.cs"))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText("namespace Test; [System.Obsolete] public class Helper { public void Work() { var x = 1; } }", path: "Helper.cs"));

		var (_, second) = RunGeneratorWithChanges<XamlGenerator>(compilation, (driver, currentCompilation) =>
		{
			var oldTree = currentCompilation.SyntaxTrees.First(tree => tree.FilePath == "Helper.cs");
			var newTree = CSharpSyntaxTree.ParseText("namespace Test; [System.Obsolete] public class Helper { public void Work() { var x = 2; } }", path: "Helper.cs");
			return (driver, currentCompilation.ReplaceSyntaxTree(oldTree, newTree));
		}, xamlFile);

		AssertStubsAreUnchanged(second);
	}

	[Fact]
	public void EditingTheMarkedHost_RedoesTheStubWork()
	{
		var xamlFile = new AdditionalXamlFile("Test.xaml", Page("<Label local:Emphasis.Level=\"3\" />"));
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts}", path: "UserCode.cs"));

		var (_, second) = RunGeneratorWithChanges<XamlGenerator>(compilation, (driver, currentCompilation) =>
		{
			var oldTree = currentCompilation.SyntaxTrees.First(tree => tree.FilePath == "UserCode.cs");
			var newTree = CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts.Replace("(\"Level\")", "(\"Intensity\")", StringComparison.Ordinal)}", path: "UserCode.cs");
			return (driver, currentCompilation.ReplaceSyntaxTree(oldTree, newTree));
		}, xamlFile);

		var reason = second.Results.Single().TrackedSteps[TrackingNames.GeneratedBindablePropertyStubsProvider].Single().Outputs.Single().Reason;
		Assert.Equal(IncrementalStepRunReason.Modified, reason);
	}

	[Fact]
	public void EditingAMethodBody_LeavesTheReferenceScanCached()
	{
		var xamlFile = new AdditionalXamlFile("Test.xaml", Page("<Label local:Emphasis.Level=\"3\" />"));
		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts}", path: "UserCode.cs"))
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText("namespace Test; public class Helper { public void Work() { var x = 1; } }", path: "Helper.cs"));

		var (_, second) = RunGeneratorWithChanges<XamlGenerator>(compilation, (driver, currentCompilation) =>
		{
			var oldTree = currentCompilation.SyntaxTrees.First(tree => tree.FilePath == "Helper.cs");
			var newTree = CSharpSyntaxTree.ParseText("namespace Test; public class Helper { public void Work() { var x = 2; } }", path: "Helper.cs");
			return (driver, currentCompilation.ReplaceSyntaxTree(oldTree, newTree));
		}, xamlFile);

		var reason = second.Results.Single().TrackedSteps[TrackingNames.MarkedAttributeNamesInReferencesProvider].Single().Outputs.Single().Reason;
		Assert.Equal(IncrementalStepRunReason.Cached, reason);
	}

	[Fact]
	public void ProjectWithoutAnyMarker_SkipsTheDeclarationScan()
	{
		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText("namespace Test; [System.Obsolete] public class Plain { }", path: "Plain.cs"));
		var referenceScan = GeneratedBindablePropertyDeclarations.GetMarkedAttributeNamesInReferences(compilation, CancellationToken.None);
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();

		// A cancelled token makes any walk over the syntax trees throw, so completing proves the walk was skipped.
		var declarations = GeneratedBindablePropertyDeclarations.GetStubDeclarations(compilation, referenceScan, cancelled.Token);

		Assert.False(referenceScan.AnyReferenceDeclaresMarker);
		Assert.Empty(declarations.Stubs);
		Assert.Empty(declarations.Problems);
	}

	[Fact]
	public void ProjectThatDeclaresTheMarker_ScansItsDeclarations()
	{
		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts}", path: "UserCode.cs"));
		var referenceScan = GeneratedBindablePropertyDeclarations.GetMarkedAttributeNamesInReferences(compilation, CancellationToken.None);
		using var cancelled = new CancellationTokenSource();
		cancelled.Cancel();

		Assert.Throws<OperationCanceledException>(() => GeneratedBindablePropertyDeclarations.GetStubDeclarations(compilation, referenceScan, cancelled.Token));
	}

	static void AssertStubsAreUnchanged(GeneratorDriverRunResult result)
	{
		var reason = result.Results.Single().TrackedSteps[TrackingNames.GeneratedBindablePropertyStubsProvider].Single().Outputs.Single().Reason;
		Assert.Contains(reason, new[] { IncrementalStepRunReason.Cached, IncrementalStepRunReason.Unchanged });
	}

	static void AssertGeneratedCodeCompilesWithSiblingSource(GeneratorDriverRunResult result)
	{
		var sources = result.Results.Single().GeneratedSources;
		var generatedTrees = sources
			.Select(source => CSharpSyntaxTree.ParseText(source.SourceText.ToString(), path: source.HintName))
			.ToArray();
		var siblingTree = CSharpSyntaxTree.ParseText(SiblingGeneratedMembers, path: "SiblingGenerated.cs");

		var compilation = CreateMauiCompilation()
			.AddSyntaxTrees(CSharpSyntaxTree.ParseText($"{MarkerAndLibraryAttributes}\n{Hosts}", path: "UserCode.cs"), siblingTree)
			.AddSyntaxTrees(generatedTrees);

		var errors = compilation.GetDiagnostics()
			.Where(d => d.Severity == DiagnosticSeverity.Error)
			.Where(d => generatedTrees.Contains(d.Location.SourceTree) || d.Location.SourceTree == siblingTree)
			.ToArray();
		Assert.Empty(errors);
	}
}
