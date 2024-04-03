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
                new TypeName("string", false, false),
                new TypeName("int", false, false),
                [
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Microsoft.Maui.Controls.Button", false, false),
                new TypeName("int", false, false),
                [
                    new MemberAccess("Text", IsNullable: false),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Foo", false, false),
                new TypeName("int", true, false),
                [
                    new MemberAccess("Button", IsNullable: true),
                    new MemberAccess("Text", IsNullable: false),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Microsoft.Maui.Controls.Button", true, false),
                new TypeName("int", true, false),
                [
                    new MemberAccess("Text", IsNullable: false),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Foo", false, false),
                new TypeName("int", true, false),
                [
                    new MemberAccess("Value", IsNullable: true),
                ],
                true
            );

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
                new TypeName("global::Microsoft.Maui.Controls.Button", true, false),
                new TypeName("int", true, false),
                [
                    new MemberAccess("Text", IsNullable: true),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Foo", true, false),
                new TypeName("int", false, false),
                [
                    new MemberAccess("Bar", IsNullable: true),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Foo", true, false),
                new TypeName("int", true, false),
                [
                    new MemberAccess("Value", IsNullable: true),
                ],
                true
            );

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
                new TypeName("global::Foo", false, false),
                new TypeName("int", false, false),
                [
                    new MemberAccess("Items", IsNullable: false),
                    new IndexAccess("Item", 0, IsNullable: false),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

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
                new TypeName("global::Foo", false, false),
                new TypeName("int", false, false),
                [
                    new MemberAccess("Items", IsNullable: false),
                    new IndexAccess("Item", "key", IsNullable: false),
                    new MemberAccess("Length", IsNullable: false),
                ],
                true
            );

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }
}