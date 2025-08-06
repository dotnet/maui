using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Diagnostics;

internal static class MauiDiagnosticsExtensions
{
	public static MauiAppBuilder ConfigureMauiDiagnostics(this MauiAppBuilder builder)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return builder;
		}

		builder.Services.AddSingleton(services => new MauiDiagnostics(services.GetServices<IDiagnosticTagger>(), services.GetService<IMeterFactory>()));
		builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDiagnosticTagger, ViewDiagnosticTagger>(_ => new ViewDiagnosticTagger()));

		return builder;
	}

	static MauiDiagnostics? GetMauiDiagnostics(this IView view)
	{
		if (!RuntimeFeature.IsMeterSupported)
		{
			return null;
		}

		return view.Handler?.MauiContext?.Services.GetService<MauiDiagnostics>();
	}

	public static Activity? StartActivity(this IView view, string name)
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

		var tagList = new TagList();
		diag.AddTags(view, ref tagList);

		var activity = diag.ActivitySource.StartActivity(
			ActivityKind.Internal,
			name: $"{name} {elementTypeName}",
			tags: tagList);

		return activity;
	}

	public static void RecordMeasure(this IView view, TimeSpan? duration)
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

		var tagList = new TagList();
		diag.AddTags(view, ref tagList);

		diag.MeasureCounter?.Add(1, tagList);

		if (duration is not null)
		{
#if NET9_0_OR_GREATER
			diag.MeasureHistogram?.Record((int)duration.Value.TotalNanoseconds, tagList);
#else
			diag.MeasureHistogram?.Record((int)(duration.Value.TotalMilliseconds * 1_000_000), tagList);
#endif
		}
	}

	public static void RecordArrange(this IView view, TimeSpan? duration)
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

		var tagList = new TagList();
		diag.AddTags(view, ref tagList);

		diag.ArrangeCounter?.Add(1, tagList);

		if (duration is not null)
		{
#if NET9_0_OR_GREATER
			diag.ArrangeHistogram?.Record((int)duration.Value.TotalNanoseconds, tagList);
#else
			diag.ArrangeHistogram?.Record((int)(duration.Value.TotalMilliseconds * 1_000_000), tagList);
#endif
		}
	}
}
