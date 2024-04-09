using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;


namespace BindingSourceGen.UnitTests;


public class BindingRepresentationGenTests
{
    [Fact]
    public void GenerateSimpleBinding()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("string"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Length"),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNestedProperties()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button b) => b.Text?.Length);
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Microsoft.Maui.Controls.Button"),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new MemberAccess("Text"),
                    new ConditionalAccess(new MemberAccess("Length")),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNullableReferenceElementInPathWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Button?.Text?.Length);

        class Foo
        {
            public Button? Button { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new MemberAccess("Button"),
                    new ConditionalAccess(new MemberAccess("Text")),
                    new ConditionalAccess(new MemberAccess("Length")),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);

    }

    [Fact]
    public void GenerateBindingWithNullableReferenceSourceWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button? b) => b?.Text?.Length);
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Microsoft.Maui.Controls.Button", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Text")),
                    new ConditionalAccess(new MemberAccess("Length")),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNullableValueTypeWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value);

        class Foo
        {
            public int? Value { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new MemberAccess("Value"),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNullableSourceReferenceAndNullableReferenceElementInPathWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button? b) => b?.Text?.Length);
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Microsoft.Maui.Controls.Button", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Text")),
                    new ConditionalAccess(new MemberAccess("Length")),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNullablePropertyReferenceWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value);

        class Foo
        {
            public string? Value { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("string", IsNullable: true),
                [
                    new MemberAccess("Value"),
                ],
                GenerateSetter: false
            );

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNullableReferenceTypesWhenNullableDisabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f?.Bar?.Length);

        class Foo
        {
            public string Bar { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 4, 7),
                new TypeDescription("global::Foo", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Bar")),
                    new ConditionalAccess(new MemberAccess("Length")),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWithNullableValueTypeWhenNullableDisabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value);

        class Foo
        {
            public int? Value { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 4, 7),
                new TypeDescription("global::Foo", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new MemberAccess("Value"),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenBindingContainsIntegerIndexing()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Items[0].Length);

        class Foo
        {
            public string[] Items { get; set; } = { "Item1" };
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Items"),
                    new IndexAccess("Item", new NumericIndex(0)),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsStringIndexing()
    {
        var source = """
        using Microsoft.Maui.Controls;
        using System.Collections.Generic;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Items["key"].Length);

        class Foo
        {
            public Dictionary<string, string> Items { get; set; } = new();
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 4, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Items"),
                    new IndexAccess("Item", new StringIndex("key")),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: false);

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsSimpleReferenceTypeCast()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value as string);

        class Foo
        {
            public object Value { get; set; } = "Value";
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("string", IsNullable: true),
                [
                    new MemberAccess("Value"),
                    new Cast(new TypeDescription("string")),
                ],
                GenerateSetter: false
            );

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsMemberAccessOfCastReferenceType()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (f.C as C)?.X);

        public class Foo
        {
            public object C { get; set; } = new C();
        }

        class C
        {
            public int X { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new MemberAccess("C"),
                    new Cast(new TypeDescription("global::C")),
                    new ConditionalAccess(new MemberAccess("X")),
                ],
                GenerateSetter: false
            );

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsMemberAccessOfCastNullableReferenceType()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (f.C as C)?.X);

        public class Foo
        {
            public object? C { get; set; }
        }

        class C
        {
            public int X { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsNullable: true, IsValueType: true),
                [
                    new MemberAccess("C"),
                    new Cast(new TypeDescription("global::C")),
                    new ConditionalAccess(new MemberAccess("X")),
                ],
                GenerateSetter: false
            );

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsSimpleValueTypeCast()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value as int?);

        class Foo
        {
            public int Value { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsNullable: true, IsValueType: true),
                [
                    new MemberAccess("Value"),
                    new Cast(new TypeDescription("int", IsNullable: true, IsValueType: true)),
                ],
                GenerateSetter: false
            );


        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsMemberAccessOfCastNullableValueType()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (f.C as C?)?.X);

        public class Foo
        {
            public object? C { get; set; }
        }

        struct C
        {
            public int X { get; set; }
        }
        """;

        var codeGeneratorResult = SourceGenHelpers.Run(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation(@"Path\To\Program.cs", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsNullable: true, IsValueType: true),
                [
                    new MemberAccess("C"),
                    new Cast(new TypeDescription("global::C", IsNullable: true, IsValueType: true)),
                    new ConditionalAccess(new MemberAccess("X")),
                ],
                GenerateSetter: false
            );

        AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
    }
}