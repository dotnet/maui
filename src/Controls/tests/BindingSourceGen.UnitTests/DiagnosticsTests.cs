using Xunit;

namespace BindingSourceGen.UnitTests;

public class DiagnosticsTests
{
    [Fact(Skip = "Improve detecting overloads")]
    public void ReportsErrorWhenGetterIsNotLambda()
    {
        var source = """
            using System;
            using Microsoft.Maui.Controls;
            var label = new Label();
            var getter = new Func<Button, int>(b => b.Text.Length);
            label.SetBinding(Label.RotationProperty, getter);
            """;

        var result = SourceGenHelpers.Run(source);
        Assert.Single(result.SourceGeneratorDiagnostics);
        Assert.Equal("BSG0002", result.SourceGeneratorDiagnostics[0].Id);
    }

    [Fact]
    public void ReportsErrorWhenLambdaBodyIsNotExpression()
    {
        var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            label.SetBinding(Label.RotationProperty, static (Button b) => { return b.Text.Length; });
            """;

        var result = SourceGenHelpers.Run(source);

        Assert.Single(result.SourceGeneratorDiagnostics);
        Assert.Equal("BSG0003", result.SourceGeneratorDiagnostics[0].Id);
    }

    [Fact]
    public void ReportsWarningWhenUsingDifferentSetBindingOverload()
    {
        var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();
            label.SetBinding(Label.ScaleProperty, new Binding("Value", source: slider));
            """;

        var result = SourceGenHelpers.Run(source);

        Assert.Single(result.SourceGeneratorDiagnostics);
        Assert.Equal("BSG0004", result.SourceGeneratorDiagnostics[0].Id);
    }

    [Fact]
    public void ReportsUnableToResolvePathWhenUsingMethodCall()
    {
        var source = """
            using Microsoft.Maui.Controls;

            double GetRotation(Button b) => b.Rotation;

            var label = new Label();
            label.SetBinding(Label.RotationProperty, (Button b) => GetRotation(b));
            """;

        var result = SourceGenHelpers.Run(source);

        Assert.Single(result.SourceGeneratorDiagnostics);
        Assert.Equal("BSG0001", result.SourceGeneratorDiagnostics[0].Id);
    }

    [Fact]
    public void ReportsUnableToResolvePathWhenUsingMultidimensionalArray()
    {
        var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();

            var array = new int[1, 1];
            label.SetBinding(Label.RotationProperty, (Button b) => array[0, 0]);
            """;

        var result = SourceGenHelpers.Run(source);

        Assert.Single(result.SourceGeneratorDiagnostics);
        Assert.Equal("BSG0001", result.SourceGeneratorDiagnostics[0].Id);
    }
}
