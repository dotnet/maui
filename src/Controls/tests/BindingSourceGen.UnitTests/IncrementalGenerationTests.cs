using Microsoft.CodeAnalysis;
using Xunit;

namespace BindingSourceGen.UnitTests;


public class IncrementalGenerationTests
{
    [Fact]
    public void CompilingTheSameSourceResultsInEqualModels()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        var inputCompilation1 = SourceGenHelpers.CreateCompilation(source);
        var driver1 = SourceGenHelpers.CreateDriver();
        var result1 = driver1.RunGenerators(inputCompilation1).GetRunResult().Results.Single();

        var inputCompilation2 = SourceGenHelpers.CreateCompilation(source);
        var driver2 = SourceGenHelpers.CreateDriver();
        var result2 = driver2.RunGenerators(inputCompilation2).GetRunResult().Results.Single();

        Assert.Equal(result1.TrackedSteps.Count, result2.TrackedSteps.Count);
        CompareGeneratorOutputs(result1, result2);
    }

    [Fact]
    public void DoesNotRegenerateCodeWhenNoChanges()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        RunGeneratorOnTwoSourcesAndVerifyResults([source], [source], reason => Assert.True(reason == IncrementalStepRunReason.Unchanged || reason == IncrementalStepRunReason.Cached));
    }

    [Fact]
    public void DoesRegenerateCodeWhenSourceChanged()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        var newSource = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s);
        """;

        RunGeneratorOnTwoSourcesAndVerifyResults([source], [newSource], reason => Assert.True(reason == IncrementalStepRunReason.Modified));
    }

    [Fact]
    public void DoesRegenerateCodeWhenNewCodeInsertedAbove()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        var newSource = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        var x = 42;
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        RunGeneratorOnTwoSourcesAndVerifyResults([source], [newSource], reason => Assert.True(reason == IncrementalStepRunReason.Modified));
    }

    [Fact]
    public void DoesNotRegenerateCodeWhenNewCodeInsertedBelow()
    {
        var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        var newSource = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);

        var x = 42;
        """;

        RunGeneratorOnTwoSourcesAndVerifyResults([source], [newSource], reason => Assert.True(reason == IncrementalStepRunReason.Unchanged || reason == IncrementalStepRunReason.Cached));
    }

    [Fact]
    public void DoesNotRegerateCodeWhenDifferentFileEdited()
    {
        var fileASource = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

        var fileBSource = """
        var x = 42;
        """;

        var fileBModified = """
        var x = 43;
        """;

        RunGeneratorOnTwoSourcesAndVerifyResults([fileASource, fileBSource], [fileASource, fileBModified], reason => Assert.True(reason == IncrementalStepRunReason.Unchanged || reason == IncrementalStepRunReason.Cached));
    }

    private static void RunGeneratorOnTwoSourcesAndVerifyResults(List<string> sources, List<string> modified, Action<IncrementalStepRunReason> assert)
    {
        var inputCompilation = SourceGenHelpers.CreateCompilation(sources);
        var cloneCompilation = inputCompilation.Clone();
        var driver = SourceGenHelpers.CreateDriver();

        var driverWithCachedInfo = driver.RunGenerators(inputCompilation);

        var result = driverWithCachedInfo.GetRunResult().Results.Single();
        var steps = result.TrackedSteps;

        var reasons = steps.SelectMany(step => step.Value).SelectMany(x => x.Outputs).Select(x => x.Reason);
        Assert.All(reasons, reason => Assert.Equal(IncrementalStepRunReason.New, reason));

        var newCompilation = SourceGenHelpers.CreateCompilation(modified);
        var newResult = driverWithCachedInfo.RunGenerators(newCompilation).GetRunResult().Results.Single();
        var newSteps = newResult.TrackedSteps;

        var newReasons = newSteps
            .Where(step => SourceGenHelpers.StepsForComparison.Contains(step.Key))
            .SelectMany(step => step.Value)
            .SelectMany(x => x.Outputs)
            .Select(x => x.Reason);

        Assert.All(newReasons, reason => assert(reason));
    }

    private static void CompareGeneratorOutputs(GeneratorRunResult result1, GeneratorRunResult result2)
    {
        var stepComparisons = from stepA in result1.TrackedSteps
                              join stepB in result2.TrackedSteps on stepA.Key equals stepB.Key
                              where SourceGenHelpers.StepsForComparison.Contains(stepA.Key)
                              select new { StepA = stepA, StepB = stepB };

        foreach (var comparison in stepComparisons)
        {
            var outputsA = comparison.StepA.Value.SelectMany(run => run.Outputs);
            var outputsB = comparison.StepB.Value.SelectMany(run => run.Outputs);

            foreach (var (outputA, outputB) in outputsA.Zip(outputsB))
            {
                Assert.Equal(outputA.Reason, outputB.Reason);
                Assert.Equal(outputA.Value, outputB.Value);
            }
        }
    }
}

