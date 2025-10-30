using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Diagnostics;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Diagnostics;

internal static class MauiControlsDiagnosticsExtensions
{
	public static MauiAppBuilder ConfigureMauiControlsDiagnostics(this MauiAppBuilder builder)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return builder;
		}

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticTagger, ControlsViewDiagnosticTagger>(_ => new ControlsViewDiagnosticTagger()));

		return builder;
	}
}
