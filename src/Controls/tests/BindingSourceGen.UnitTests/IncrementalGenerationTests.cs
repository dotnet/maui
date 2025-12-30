using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.BindingSourceGen;
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
		var driver1 = SourceGenHelpers.CreateDriver([new BindingSourceGenerator().AsSourceGenerator()]);
		var result1 = driver1.RunGenerators(inputCompilation1).GetRunResult().Results.Single();

		var inputCompilation2 = SourceGenHelpers.CreateCompilation(source);
		var driver2 = SourceGenHelpers.CreateDriver([new BindingSourceGenerator().AsSourceGenerator()]);
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

		var results = RunGeneratorOnMultipleSourcesAndReturnSteps(
			new Dictionary<string, string> { { nameof(source), source } },
			new Dictionary<string, string> { { nameof(source), source } });

		var outputs = results[nameof(source)].SelectMany(step => step.Outputs);
		Assert.All(outputs, output => Assert.True(output.Reason == IncrementalStepRunReason.Unchanged || output.Reason == IncrementalStepRunReason.Cached));
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

		var results = RunGeneratorOnMultipleSourcesAndReturnSteps(
			new Dictionary<string, string> { { nameof(source), source } },
			new Dictionary<string, string> { { nameof(source), newSource } });

		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(source)], TrackingNames.BindingsWithDiagnostics);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(source)], TrackingNames.Bindings);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Unchanged, results[nameof(source)], "SourceOutput");
		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(source)], "ImplementationSourceOutput");
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

		var results = RunGeneratorOnMultipleSourcesAndReturnSteps(
			new Dictionary<string, string> { { nameof(source), source } },
			new Dictionary<string, string> { { nameof(source), newSource } });

		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(source)], TrackingNames.BindingsWithDiagnostics);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(source)], TrackingNames.Bindings);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Unchanged, results[nameof(source)], "SourceOutput");
		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(source)], "ImplementationSourceOutput");
	}

	private static void AssertStepRunReasonEquals(IncrementalStepRunReason expectedReason, IncrementalGeneratorRunStep[] steps, string stepName)
	{
		Assert.Equal(expectedReason, steps.Single(r => r.Name == stepName).Outputs.Single().Reason);
	}

	[Fact]
	public void DoesNotRegerateCodeWhenDifferentFileEdited()
	{
		var fileASource = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Label l) => l.Text.Length);
        """;

		var fileBSource = """
        using Microsoft.Maui.Controls;
        var button = new Button();
        button.SetBinding(Button.RotationProperty, static (Button b) => b.Text.Length);
        """;

		var fileBModified = """
        using Microsoft.Maui.Controls;
        var button = new Button();
        button.SetBinding(Button.RotationProperty, static (Button b) => b.Text);
        """;

		var results = RunGeneratorOnMultipleSourcesAndReturnSteps(
			new Dictionary<string, string> { { nameof(fileASource), fileASource }, { nameof(fileBSource), fileBSource } },
			new Dictionary<string, string> { { nameof(fileASource), fileASource }, { nameof(fileBSource), fileBModified } });

		AssertStepRunReasonEquals(IncrementalStepRunReason.Unchanged, results[nameof(fileASource)], TrackingNames.BindingsWithDiagnostics);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Cached, results[nameof(fileASource)], TrackingNames.Bindings);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Cached, results[nameof(fileASource)], "SourceOutput");
		AssertStepRunReasonEquals(IncrementalStepRunReason.Cached, results[nameof(fileASource)], "ImplementationSourceOutput");

		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(fileBSource)], TrackingNames.BindingsWithDiagnostics);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(fileBSource)], TrackingNames.Bindings);
		AssertStepRunReasonEquals(IncrementalStepRunReason.Unchanged, results[nameof(fileBSource)], "SourceOutput");
		AssertStepRunReasonEquals(IncrementalStepRunReason.Modified, results[nameof(fileBSource)], "ImplementationSourceOutput");
	}

	private static Dictionary<string, IncrementalGeneratorRunStep[]> RunGeneratorOnMultipleSourcesAndReturnSteps(
		Dictionary<string, string> initialSources,
		Dictionary<string, string> modifiedSources)
	{
		var inputCompilation = SourceGenHelpers.CreateCompilation(initialSources);
		var cloneCompilation = inputCompilation.Clone();
		var driver = SourceGenHelpers.CreateDriver([new BindingSourceGenerator().AsSourceGenerator()]);

		var driverWithCachedInfo = driver.RunGenerators(inputCompilation);

		var initialResult = driverWithCachedInfo.GetRunResult().Results.Single();
		var initialSteps = initialResult.TrackedSteps;

		var initialReasons = initialSteps.SelectMany(step => step.Value)
									 .SelectMany(runStep => runStep.Outputs)
									 .Select(output => output.Reason);

		var newCompilation = SourceGenHelpers.CreateCompilation(modifiedSources);
		var newResult = driverWithCachedInfo.RunGenerators(newCompilation).GetRunResult().Results.Single();
		var newSteps = newResult.TrackedSteps;

		// Single step runs, e.g. SourceOutput-fileA, SourceOuput-fileB
		var runs = newSteps.SelectMany(step => step.Value);

		// Pairs <binding, run>. Note that a single run can be associated with multiple bindings.
		// In such cases generate <binding, run> pair for each binding.
		var bindingRunPairs = runs
			.Select(run => (GetBindingInvocationDescription(run), run))
			.SelectMany(bindingsRunPair => bindingsRunPair.Item1.Select(binding => (binding, bindingsRunPair.run)));


		// Sometimes the binding has more than one run of the same step associated with it. 
		// In such cases keep the one with Modified reason for safety.
		return bindingRunPairs
		.GroupBy(bindingRunPair => bindingRunPair.binding.SimpleLocation!.FilePath)
		.ToDictionary(
			x => x.Key,
			x => x
				.GroupBy(y => y.run.Name)
				.Select(g => g
					.FirstOrDefault(y => y.run.Outputs[0].Reason == IncrementalStepRunReason.Modified).run ?? g.First().run)
				.ToArray());
	}

	private static BindingInvocationDescription[] GetBindingInvocationDescription(IncrementalGeneratorRunStep step)
	{
		var bindingCandidate = step switch
		{
			{ Name: TrackingNames.BindingsWithDiagnostics } => step.Outputs[0].Value,
			{ Name: TrackingNames.Bindings } => step.Outputs[0].Value,
			{ Name: "SourceOutput" } => step.Inputs[0].Source.Outputs[0].Value,
			{ Name: "ImplementationSourceOutput" } => step.Inputs[0].Source.Outputs[0].Value,
			_ => null
		};

		return bindingCandidate switch
		{
			BindingInvocationDescription => [(BindingInvocationDescription)bindingCandidate],
			Result<BindingInvocationDescription> => [((Result<BindingInvocationDescription>)bindingCandidate).Value],
			ImmutableArray<BindingInvocationDescription> => ((ImmutableArray<BindingInvocationDescription>)bindingCandidate).ToArray(),
			_ => []
		};
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
