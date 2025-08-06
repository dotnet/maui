using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Diagnostics;

internal class MauiDiagnostics
{
    public MauiDiagnostics(IMeterFactory? meterFactory = null)
    {
        ActivitySource = new ActivitySource("Microsoft.Maui.Diagnostics", "1.0.0");

        Meters = meterFactory?.Create("Microsoft.Maui.Diagnostics", "1.0.0");

        MeasureCounter = Meters?.CreateCounter<int>("maui.layout.measure_count", "{times}", "The number of times a measure happened.");
        ArrangeCounter = Meters?.CreateCounter<int>("maui.layout.arrange_count", "{times}", "The number of times an arrange happened.");

        MeasureHistogram = Meters?.CreateHistogram<int>("maui.layout.measure_duration", "ns");
        ArrangeHistogram = Meters?.CreateHistogram<int>("maui.layout.arrange_duration", "ns");
    }

	public ActivitySource ActivitySource { get; }
	public Meter? Meters { get; }
	public Counter<int>? MeasureCounter { get; }
	public Counter<int>? ArrangeCounter { get; }
	public Histogram<int>? MeasureHistogram { get; }
	public Histogram<int>? ArrangeHistogram { get; }
}

internal static class MauiDiagnosticsExtensions
{
	public static MauiAppBuilder ConfigureMauiDiagnostics(this MauiAppBuilder builder)
	{
        if (RuntimeFeature.IsMeterSupported)
        {
            builder.Services.AddSingleton(serviceProvider => new MauiDiagnostics(serviceProvider.GetService<IMeterFactory>()));
        }

		return builder;
	}

	static MauiDiagnostics? GetMauiDiagnostics(this IView view)
	{
        if (!RuntimeFeature.IsMeterSupported)
            return null;

		return view.Handler?.MauiContext?.Services.GetService<MauiDiagnostics>();
	}

	public static Activity? StartActivity(this IView view, string name)
	{
        if (!RuntimeFeature.IsMeterSupported)
            return null;

		var elementName = view.GetType().Name;

		var activity = view.GetMauiDiagnostics()?.ActivitySource.StartActivity($"{name} {elementName}");

		activity?.SetTag("element.type", view.GetType().FullName);

		return activity;
	}

	public static void RecordMeasure(this IView view, TimeSpan? duration)
	{
        if (!RuntimeFeature.IsMeterSupported)
            return;

		var diag = view.GetMauiDiagnostics();
		diag?.MeasureCounter?.Add(1);

        if (duration is not null)
		    diag?.MeasureHistogram?.Record((int)duration.Value.TotalNanoseconds);
	}

	public static void RecordArrange(this IView view, TimeSpan? duration)
	{
        if (!RuntimeFeature.IsMeterSupported)
            return;

		var diag = view.GetMauiDiagnostics();
		diag?.ArrangeCounter?.Add(1);
        
		if (duration is not null)
            diag?.ArrangeHistogram?.Record((int)duration.Value.TotalNanoseconds);
	}
}