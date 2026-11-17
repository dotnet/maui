#nullable enable

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class SourceGenContextTests
{
	[Fact]
	public void BufferedDiagnostics_AreForwardedOnlyWhenFlushed()
	{
		var forwardedDiagnostics = new List<Diagnostic>();
		var context = SourceGenContext.CreateNewForTests(forwardedDiagnostics.Add);
		var childContext = SourceGenContext.CreateNewForTests();
		childContext.ParentContext = context;
		var discardedDiagnostic = Diagnostic.Create(Descriptors.XamlParserError, Location.None, "discarded");

		context.BeginDiagnosticBuffering();
		childContext.ReportDiagnostic(discardedDiagnostic);

		Assert.Empty(forwardedDiagnostics);
		Assert.Equal(1, context.BufferedDiagnosticCount);

		context.DiscardBufferedDiagnostics();
		Assert.Empty(forwardedDiagnostics);
		Assert.Equal(0, context.BufferedDiagnosticCount);

		var flushedDiagnostic = Diagnostic.Create(Descriptors.XamlParserError, Location.None, "flushed");
		context.BeginDiagnosticBuffering();
		context.ReportDiagnostic(flushedDiagnostic);
		context.FlushBufferedDiagnostics();

		Assert.Equal([flushedDiagnostic], forwardedDiagnostics);
		Assert.Equal(0, context.BufferedDiagnosticCount);
	}
}
