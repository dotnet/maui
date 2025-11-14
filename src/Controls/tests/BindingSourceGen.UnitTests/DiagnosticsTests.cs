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
            label.SetBinding(Label.RotationProperty, static (Button b) => GetRotation(b));
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

			label.SetBinding(Label.RotationProperty, static (Foo b) => b.array[0, 0]);

			class Foo
			{
				int[,] array = new int[1, 1];
			}
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
			label.SetBinding(Label.RotationProperty, static (ClassA a) => a.CounterBtn.Text.Length);
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
						label.SetBinding(Label.RotationProperty, static (ClassA a) => a.CounterBtn.Text.Length);
					}
				}
			}

			""";

		var result = SourceGenHelpers.Run(source, [new BindingSourceGenerator(), new IncrementalGeneratorA()]);

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0006", diagnostic.Id);
		AssertExtensions.AssertNoDiagnostics(result.GeneratedCodeCompilationDiagnostics, "Generated code compilation");
	}

	[Theory]
	[InlineData("private")]
	[InlineData("protected")]
	[InlineData("private protected")]
	// https://github.com/dotnet/maui/issues/23534
	public void SupportsInaccessibleSourceType(string modifier)
	{
		// Previously this test checked for BSG0007 error
		// Now we support private types using UnsafeAccessorType
		var source = $$"""
			using Microsoft.Maui.Controls;

			var foo = new Foo();
			foo.Bar();

			public class Foo
			{
				public void Bar()
				{
					var label = new Label();
					label.SetBinding(Label.RotationProperty, static (UnaccessibleClass a) => a.Value);
				}

				{{modifier}} class UnaccessibleClass
				{
					public int Value { get; set; }
				}
			}
			""";

		var result = SourceGenHelpers.Run(source);

		// Should not have any diagnostics - private types are now supported
		AssertExtensions.AssertNoDiagnostics(result);
		Assert.NotNull(result.Binding);
	}

	[Fact]
	public void ReportsWarningWhenLambdaIsNotStatic()
	{
		var source = """
			using Microsoft.Maui.Controls;

			var label = new Label();
			var text = "Hello";
			label.SetBinding(Label.RotationProperty, (Button b) => text.Length);
			""";

		var result = SourceGenHelpers.Run(source);

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0010", diagnostic.Id);
		AssertExtensions.AssertNoDiagnostics(result.GeneratedCodeCompilationDiagnostics, "Generated code compilation");
	}

	[Fact]
	public void ReportsWarningWhenLambdaHasNoParameters()
	{
		var source = """
			using Microsoft.Maui.Controls;

			label.SetBinding(Label.RotationProperty, static () => text.Length);
			""";

		var result = SourceGenHelpers.Run(source);

		var diagnostic = Assert.Single(result.SourceGeneratorDiagnostics);
		Assert.Equal("BSG0005", diagnostic.Id);
	}

	[Fact]
	public void DoesNotReportWarningWhenUsingBindablePropertyCreateWithLambdaInPropertyChanged()
	{
		var source = """
			using Microsoft.Maui.Controls;
		
			public partial class TimelineBaseView : Grid
			{
				public TimelineBaseView()
				{
					InitializeComponent();
				}

				public static readonly BindableProperty MainContentProperty
					= BindableProperty.Create(nameof(MainContent), typeof(View), typeof(TimelineBaseView), default(View),
						propertyChanged: (obj, oldValue, newValue) =>
						{
							if (obj is TimelineBaseView self)
								self.mainContent.Content = newValue as View;
						});

				public View MainContent
				{
					get => (View)GetValue(MainContentProperty);
					set => SetValue(MainContentProperty, value);
				}
			}

			public static class Program
			{
				public static void Main()
				{
					var timelineBaseView = new TimelineBaseView();
				}
			}
			""";

		var result = SourceGenHelpers.Run(source);
		Assert.Empty(result.SourceGeneratorDiagnostics);
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
