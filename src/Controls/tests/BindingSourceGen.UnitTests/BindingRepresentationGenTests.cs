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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("string", false, false),
                new TypeName("int", false, false),
                [
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("global::Microsoft.Maui.Controls.Button", false, false),
                new TypeName("int", false, false),
                [
                    new PathPart("Text", false),
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("global::Foo", false, false),
                new TypeName("int", true, false),
                [
                    new PathPart("Button", true),
                    new PathPart("Text", false),
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("global::Microsoft.Maui.Controls.Button", true, false),
                new TypeName("int", true, false),
                [
                    new PathPart("Text", false),
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("global::Foo", false, false),
                new TypeName("int", true, false),
                [
                    new PathPart("Value", true),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("global::Microsoft.Maui.Controls.Button", true, false),
                new TypeName("int", true, false),
                [
                    new PathPart("Text", true),
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 };
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 4, 7),
                new TypeName("global::Foo", true, false),
                new TypeName("int", false, false),
                [
                    new PathPart("Bar", true),
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 };
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 4, 7),
                new TypeName("global::Foo", true, false),
                new TypeName("int", true, false),
                [
                    new PathPart("Value", true),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 3, 7),
                new TypeName("global::Foo", false, false),
                new TypeName("int", false, false),
                [
                    new PathPart("Items", false, 0),
                    new PathPart("Length", false),
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

        var actualBinding = SourceGenHelpers.GetBinding(source) with { Id = 0 }; // TODO: Improve indexing of bindings
        var expectedBinding = new CodeWriterBinding(
                0,
                new SourceCodeLocation("", 4, 7),
                new TypeName("global::Foo", false, false),
                new TypeName("int", false, false),
                [
                    new PathPart("Items", false, "key"),
                    new PathPart("Length", false),
                ],
                true
            );

        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }
}