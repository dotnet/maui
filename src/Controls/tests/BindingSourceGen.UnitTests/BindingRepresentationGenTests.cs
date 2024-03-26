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

        var result = SourceGenHelpers.Run(source);
        var results = result.Results.Single();
        var steps = results.TrackedSteps;
        var actualBinding = (CodeWriterBinding)steps["Bindings"][0].Outputs[0].Value;

        actualBinding = actualBinding with { Id = 0 }; // TODO: Improve indexing of bindings

        var sourceCodeLocation = new SourceCodeLocation("", 3, 7);

        var expectedBinding = new CodeWriterBinding(
                0,
                sourceCodeLocation,
                new TypeName("string", false, false),
                new TypeName("int", false, false),
                [
                    new PathPart("s", false),
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

        var result = SourceGenHelpers.Run(source);
        var results = result.Results.Single();
        var steps = results.TrackedSteps;
        var actualBinding = (CodeWriterBinding)steps["Bindings"][0].Outputs[0].Value;

        var sourceCodeLocation = new SourceCodeLocation("", 3, 7);

        actualBinding = actualBinding with { Id = 0 }; // TODO: Improve indexing of bindings

        var expectedBinding = new CodeWriterBinding(
                0,
                sourceCodeLocation,
                new TypeName("global::Microsoft.Maui.Controls.Button", false, false),
                new TypeName("int", false, false),
                [
                    new PathPart("b", false),
                    new PathPart("Text", false),
                    new PathPart("Length", false),
                ],
                true
            );
        //TODO: Change arrays to custom collections implementing IEquatable
        Assert.Equal(expectedBinding.Path, actualBinding.Path);
        Assert.Equivalent(expectedBinding, actualBinding, strict: true);
    }
}