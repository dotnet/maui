using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

public class DiagnosticsTests
{
	[Fact]
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

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0002", diagnostic.Id);
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

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0003", diagnostic.Id);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingOverloadWithBindingClassDeclaredInInvocation()
	{
		var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();
            label.SetBinding(Label.ScaleProperty, new Binding("Value", source: slider));
            """;

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingOverloadWithBindingClassPassedAsVariable()
	{
		var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();
            var binding = new Binding("Value", source: slider);
            label.SetBinding(Label.ScaleProperty, binding);
            """;

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingOverloadWithStringConstantPath()
	{
		var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();

            label.BindingContext = slider;
            label.SetBinding(Label.ScaleProperty, "Value");
            """;

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingOverloadWithStringVariablePath()
	{
		var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();

            label.BindingContext = slider;
            var str = "Value";
            label.SetBinding(Label.ScaleProperty, str);
            """;

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingOverloadWithNameofInPath()
	{
		var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();

            label.BindingContext = slider;
            label.SetBinding(Label.ScaleProperty, nameof(Slider.Value));
            """;

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingOverloadWithMethodCallThatReturnsString()
	{
		var source = """
            using Microsoft.Maui.Controls;
            var label = new Label();
            var slider = new Slider();

            label.BindingContext = slider;
            label.SetBinding(Label.ScaleProperty, GetPath());
            
            static string GetPath() => "Value";
            """;

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
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

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0001", diagnostic.Id);
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

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0001", diagnostic.Id);
	}

	[Fact]
	// https://github.com/dotnet/maui/issues/23531
	public void ReportsWarningWhenLambdaParameterIsSourceGeneratedType()
	{
		var source = """
			using Microsoft.Maui.Controls;
			using SomeNamespace;

			var label = new Label();
			label.SetBinding(Label.RotationProperty, (ClassA a) => a.CounterBtn.Text.Length);
			""";

		var result = SourceGenHelpers.Run(source, [new BindingSourceGenerator(), new IncrementalGeneratorA()]);

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0005", diagnostic.Id);
		AssertExtensions.AssertNoDiagnostics(result.GeneratedCodeCompilationDiagnostics, "Generated code compilation");
	}

	[Fact]
	// https://github.com/dotnet/maui/issues/23531
	public void ReportsWarningWhenPathContainsSourceGeneratedMember()
	{
		var source = """
			using Microsoft.Maui.Controls;

			var a = new SomeNamespace.ClassA();
			a.Foo();

			namespace SomeNamespace
			{
				public partial class ClassA
				{
					public void Foo()
					{
						var label = new Label();
						label.SetBinding(Label.RotationProperty, (ClassA a) => a.CounterBtn.Text.Length);
					}
				}
			}

			""";

		var result = SourceGenHelpers.Run(source, [new BindingSourceGenerator(), new IncrementalGeneratorA()]);

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0006", diagnostic.Id);
		AssertExtensions.AssertNoDiagnostics(result.GeneratedCodeCompilationDiagnostics, "Generated code compilation");
	}
}

internal class IncrementalGeneratorA : IIncrementalGenerator
{
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		context.RegisterSourceOutput(
			context.CompilationProvider,
			(ctx, compilation) =>
			{
				var source = """
				using Microsoft.Maui.Controls;
				namespace SomeNamespace
				{
					public partial class ClassA
					{
						public Button CounterBtn;
					}
				}
				""";
				ctx.AddSource("SampleSourceGeneratorOutput.g.cs", SourceText.From(source, Encoding.UTF8));
			});
	}
}
