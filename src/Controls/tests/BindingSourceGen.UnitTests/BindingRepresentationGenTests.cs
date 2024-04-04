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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("string"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }

    [Fact]
    public void GenerateBindingWithNestedProperties()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button b) => b.Text.Length);
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Microsoft.Maui.Controls.Button"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Text"),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }

    [Fact]
    public void GenerateBindingWithNullableReferenceElementInPathWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Button?.Text.Length);

        class Foo
        {
            public Button? Button { get; set; }
        }
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Button")),
                    new MemberAccess("Text"),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);

    }

    [Fact]
    public void GenerateBindingWithNullableReferenceSourceWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button? b) => b?.Text.Length);
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Microsoft.Maui.Controls.Button", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new MemberAccess("Text"),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Value")),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }

    [Fact]
    public void GenerateBindingWithNullableSourceReferenceAndNullableReferenceElementInPathWhenNullableEnabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button? b) => b?.Text?.Length);
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Microsoft.Maui.Controls.Button", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Text")),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }

    [Fact]
    public void GenerateBindingWithNullableReferenceTypesWhenNullableDisabled()
    {
        var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Bar.Length);

        class Foo
        {
            public string Bar { get; set; }
        }
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 4, 7),
                new TypeDescription("global::Foo", IsNullable: true),
                new TypeDescription("int", IsValueType: true),
                [
                    new ConditionalAccess(new MemberAccess("Bar")),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 4, 7),
                new TypeDescription("global::Foo", IsNullable: true),
                new TypeDescription("int", IsValueType: true, IsNullable: true),
                [
                    new ConditionalAccess(new MemberAccess("Value")),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Items"),
                    new IndexAccess("Item", new NumericIndex(0)),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 4, 7),
                new TypeDescription("global::Foo"),
                new TypeDescription("int", IsValueType: true),
                [
                    new MemberAccess("Items"),
                    new IndexAccess("Item", new StringIndex("key")),
                    new MemberAccess("Length"),
                ],
                GenerateSetter: true);

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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
            public object Value { get; set; }
        }
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo", IsNullable: false, IsGenericParameter: false, IsValueType: false),
                new TypeDescription("string", IsNullable: false, IsGenericParameter: false, IsValueType: false),
                [
                    new Cast(
                        new MemberAccess("Value"),
                        new TypeDescription("string", IsNullable: false, IsGenericParameter: false, IsValueType: false)
                        ),
                ],
                GenerateSetter: true
            );

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }

    [Fact]
    public void GenerateBindingWhenGetterContainsMemberAccessOfCastReferenceType()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (f.C as C).X);

        public class Foo
        {
            public object C { get; set; }
        }

        class C
        {
            public int X { get; set; }
        }
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo", IsNullable: false, IsGenericParameter: false, IsValueType: false),
                new TypeDescription("int", IsNullable: false, IsGenericParameter: false, IsValueType: true),
                [
                    new Cast(
                        new MemberAccess("C"),
                        new TypeDescription("global::C", IsNullable: false, IsGenericParameter: false, IsValueType: false)
                        ),
                    new MemberAccess("X"),
                ],
                true
            );

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo", IsNullable: false, IsGenericParameter: false, IsValueType: false),
                new TypeDescription("int", IsNullable: true, IsGenericParameter: false, IsValueType: true),
                [
                    new Cast(
                        new ConditionalAccess(new MemberAccess("C")),
                        new TypeDescription("global::C", IsNullable: false, IsGenericParameter: false, IsValueType: false)
                        ),
                    new MemberAccess("X"),
                ],
                true
            );

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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
            public double Value { get; set; }
        }
        """;

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo", IsNullable: false, IsGenericParameter: false, IsValueType: false),
                new TypeDescription("int", IsNullable: true, IsGenericParameter: false, IsValueType: true),
                [
                    new Cast(
                        new MemberAccess("Value"),
                        new TypeDescription("int", IsNullable: true, IsGenericParameter: false, IsValueType: true)
                        ),
                ],
                true
            );


        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
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

        var actualBinding = SourceGenHelpers.GetBinding(source);
        var expectedBinding = new CodeWriterBinding(
                new SourceCodeLocation("", 3, 7),
                new TypeDescription("global::Foo", IsNullable: false, IsGenericParameter: false, IsValueType: false),
                new TypeDescription("int", IsNullable: true, IsGenericParameter: false, IsValueType: true),
                [
                    new Cast(
                        new ConditionalAccess(new MemberAccess("C")),
                        new TypeDescription("global::C", IsNullable: true, IsGenericParameter: false, IsValueType: true)
                        ),
                    new MemberAccess("X"),
                ],
                true
            );

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }
}