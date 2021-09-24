using System.Diagnostics;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
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