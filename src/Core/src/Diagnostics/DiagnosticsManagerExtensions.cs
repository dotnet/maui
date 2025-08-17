using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Diagnostics;

internal static class DiagnosticsManagerExtensions
{
	public static MauiAppBuilder ConfigureMauiDiagnostics(this MauiAppBuilder builder)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return builder;
		}

		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticTagger, ViewDiagnosticTagger>(_ => new ViewDiagnosticTagger()));
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticMetrics, LayoutDiagnosticMetrics>(_ => new LayoutDiagnosticMetrics()));

		builder.Services.AddSingleton<IDiagnosticsManager>(services => new DiagnosticsManager(
			services.GetServices<IDiagnosticMetrics>(),
			services.GetServices<IDiagnosticTagger>(),
			services.GetService<IMeterFactory>()));

		return builder;
	}

	internal static IDiagnosticsManager? GetMauiDiagnostics(this IView view)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return null;
		}

		return view.Handler?.MauiContext?.Services?.GetService<IDiagnosticsManager>();
	}

	internal static Activity? StartDiagnosticActivity(this IView view, string name)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return null;
		}

		var diag = view.GetMauiDiagnostics();
		if (diag is null)
		{
			return null;
		}

		var elementTypeName = view.GetType().Name;

		diag.GetTags(view, out var tagList);

		var activity = diag.ActivitySource.StartActivity(
			ActivityKind.Internal,
			name: $"{name} {elementTypeName}",
			tags: tagList);

		return activity;
	}

	internal static void StopDiagnostics(this IView view, Activity? activity, IDiagnosticInstrumentation? instrumentation = null)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return;
		}

		var diag = view.GetMauiDiagnostics();
		if (diag is null)
		{
			return;
		}

		activity?.Stop();

		if (instrumentation is not null)
		{
			diag.GetTags(view, out var tagList);
			instrumentation.Stopped(diag, in tagList);
		}

		activity?.Dispose();
	}
}
