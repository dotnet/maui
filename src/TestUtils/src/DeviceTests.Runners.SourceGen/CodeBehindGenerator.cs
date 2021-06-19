using System;
using Microsoft.CodeAnalysis;
using System.Diagnostics;

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
			if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.TargetFramework", out var targetFramework))
				return;

			context.Log($"TargetFramework: {targetFramework}");

			var generator = new RunnerGenerator(context, targetFramework);

			generator?.Generate();
		}
	}

	static class GeneratorDiagnostics
	{
		public static readonly DiagnosticDescriptor LoggingMessage = new DiagnosticDescriptor(
			id: "TST1001",
			title: "Logging Message",
			messageFormat: "{0}",
			category: "Logging",
			DiagnosticSeverity.Info,
			isEnabledByDefault: true);

		[Conditional("DEBUG")]
		public static void Log(this GeneratorExecutionContext context, string message) =>
			context.ReportDiagnostic(Diagnostic.Create(LoggingMessage, Location.None, message));
	}
}