using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
	[Generator]
	public class CodeBehindGenerator : ISourceGenerator
	{
		public void Initialize(GeneratorInitializationContext context)
		{
			//#if DEBUG
			//if (!System.Diagnostics.Debugger.IsAttached)
			//	System.Diagnostics.Debugger.Launch();
			//#endif
		}

		public void Execute(GeneratorExecutionContext context)
		{
			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TestRunnerGenerator", out var testRunnerGenerator);
			if (!Enum.TryParse<GeneratorType>(testRunnerGenerator, out var generatorType))
				generatorType = GeneratorType.None;

			if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework))
				return;

			RunnerGenerator? generator = generatorType switch
			{
				GeneratorType.Headless => new HeadlessRunnerGenerator(context, targetFramework),
				GeneratorType.Visual => new VisualRunnerGenerator(context, targetFramework),
				_ => null,
			};

			generator?.Generate();
		}
	}
}