using System;
using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

internal interface IDiagnosticInstrumentation : IDisposable
{
	void Record(MauiDiagnostics diag, in TagList tagList);
}
