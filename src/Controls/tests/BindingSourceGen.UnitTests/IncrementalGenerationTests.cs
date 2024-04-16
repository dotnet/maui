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

        var inputCompilation = SourceGenHelpers.CreateCompilation(source);
        var inputCompilationClone = inputCompilation.Clone();
        var driver = SourceGenHelpers.CreateDriver();

        var driverWithCachedInfo = driver.RunGenerators(inputCompilation);

        var result = driverWithCachedInfo.GetRunResult().Results.Single();
        var steps = result.TrackedSteps;

        var reasons = steps.SelectMany(step => step.Value).SelectMany(x => x.Outputs).Select(x => x.Reason);
        Assert.All(reasons, reason => Assert.Equal(IncrementalStepRunReason.New, reason));

        result = driverWithCachedInfo.RunGenerators(inputCompilationClone).GetRunResult().Results.Single();
        steps = result.TrackedSteps;

        reasons = steps
            .Where(step => SourceGenHelpers.StepsForComparison.Contains(step.Key))
            .SelectMany(step => step.Value)
            .SelectMany(x => x.Outputs)
            .Select(x => x.Reason);

        Assert.All(reasons, reason => Assert.True(reason == IncrementalStepRunReason.Unchanged || reason == IncrementalStepRunReason.Cached));
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

