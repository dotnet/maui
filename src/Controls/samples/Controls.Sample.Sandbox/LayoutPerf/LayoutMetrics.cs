using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Maui.Controls.Sample;

/// <summary>
/// Collects the layout measure/arrange counters that .NET MAUI already publishes from
/// <c>src/Core/src/Diagnostics/Instrumentation/LayoutDiagnosticMetrics.cs</c>
/// (<c>maui.layout.measure_count</c>, <c>maui.layout.arrange_count</c>, tagged with <c>element.type</c>).
///
/// Nothing in the framework is modified: the sample only supplies the <see cref="IMeterFactory"/> that
/// <c>DiagnosticsManagerExtensions.ConfigureMauiDiagnostics</c> looks for, and subscribes a
/// <see cref="MeterListener"/> on demand.
///
/// The listener is started only for the "counting" pass so the "timing" pass measures wall-clock cost with
/// unsubscribed (cheap) counters.
/// </summary>
public static class LayoutMetrics
{
	const string MeterName = "Microsoft.Maui";
	const string ActivitySourceName = "Microsoft.Maui";
	const string HandlerActivitySourceName = "Microsoft.Maui.Handlers";
	const string MeasureCounter = "maui.layout.measure_count";
	const string ArrangeCounter = "maui.layout.arrange_count";
	const string MeasureHistogram = "maui.layout.measure_duration";
	const string ArrangeHistogram = "maui.layout.arrange_duration";

	static readonly Dictionary<string, int> s_measureByType = new(StringComparer.Ordinal);
	static readonly Dictionary<string, int> s_arrangeByType = new(StringComparer.Ordinal);
	static readonly Dictionary<string, long> s_measureNsByType = new(StringComparer.Ordinal);
	static readonly Dictionary<string, int> s_handlerCountByOperation = new(StringComparer.Ordinal);
	static readonly Dictionary<string, long> s_handlerNsByOperation = new(StringComparer.Ordinal);
	static readonly Dictionary<string, int> s_mapperCountByProperty = new(StringComparer.Ordinal);
	static readonly Dictionary<string, long> s_mapperNsByProperty = new(StringComparer.Ordinal);

	static MeterListener? s_listener;
	static ActivityListener? s_activityListener;
	static bool s_includeDurations;
	static bool s_includeHandlerDurations;
	static int s_measureTotal;
	static int s_arrangeTotal;
	static long s_measureNsTotal;
	static long s_arrangeNsTotal;

	public static bool IsListening => s_listener is not null;

	public static int MeasureTotal => s_measureTotal;

	public static int ArrangeTotal => s_arrangeTotal;

	public static long MeasureNsTotal => s_measureNsTotal;

	public static long ArrangeNsTotal => s_arrangeNsTotal;

	public static long HandlerNsTotal =>
		s_handlerNsByOperation.TryGetValue("SetVirtualView", out long total) ? total : 0;

	/// <summary>Minimal <see cref="IMeterFactory"/> so the MAUI DiagnosticsManager can create its Meter.</summary>
	public sealed class SandboxMeterFactory : IMeterFactory
	{
		readonly List<Meter> _meters = new();

		public Meter Create(MeterOptions options)
		{
			var meter = new Meter(options.Name, options.Version, options.Tags, scope: this);
			_meters.Add(meter);
			return meter;
		}

		public void Dispose()
		{
			for (int i = 0; i < _meters.Count; i++)
			{
				_meters[i].Dispose();
			}

			_meters.Clear();
		}
	}

	/// <summary>
	/// Also attaches an <see cref="ActivityListener"/>, which is what makes
	/// <c>LayoutMeasureInstrumentation</c> produce a non-null Activity and therefore populate the
	/// framework's <c>maui.layout.measure_duration</c> histogram. Durations are Activity durations, so a
	/// container's number includes its children; leaf types (Label, BoxView) are exclusive.
	/// This pass is more intrusive than plain counting, so it is run separately from the timing pass.
	/// </summary>
	public static void StartListening(bool includeDurations = false, bool includeHandlerDurations = false)
	{
		if (s_listener is not null)
		{
			return;
		}

		s_includeDurations = includeDurations;
		s_includeHandlerDurations = includeHandlerDurations;

		if (includeDurations || includeHandlerDurations)
		{
			var activityListener = new ActivityListener
			{
				ShouldListenTo = static source =>
					(s_includeDurations && string.Equals(source.Name, ActivitySourceName, StringComparison.Ordinal)) ||
					(s_includeHandlerDurations && string.Equals(source.Name, HandlerActivitySourceName, StringComparison.Ordinal)),
				Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
				SampleUsingParentId = static (ref ActivityCreationOptions<string> _) => ActivitySamplingResult.AllData,
				ActivityStopped = static activity =>
				{
					if (string.Equals(activity.Source.Name, HandlerActivitySourceName, StringComparison.Ordinal))
					{
						OnHandlerActivityStopped(activity);
					}
				},
			};

			ActivitySource.AddActivityListener(activityListener);
			s_activityListener = activityListener;
		}

		var listener = new MeterListener();
		listener.InstrumentPublished = static (instrument, l) =>
		{
			if (!string.Equals(instrument.Meter.Name, MeterName, StringComparison.Ordinal))
			{
				return;
			}

			// Counters always; histograms only in the duration pass (they need the ActivityListener).
			if (string.Equals(instrument.Name, MeasureCounter, StringComparison.Ordinal) ||
				string.Equals(instrument.Name, ArrangeCounter, StringComparison.Ordinal) ||
				(s_includeDurations &&
					(string.Equals(instrument.Name, MeasureHistogram, StringComparison.Ordinal) ||
					string.Equals(instrument.Name, ArrangeHistogram, StringComparison.Ordinal))))
			{
				l.EnableMeasurementEvents(instrument);
			}
		};

		listener.SetMeasurementEventCallback<int>(OnMeasurement);
		listener.Start();
		s_listener = listener;
	}

	public static void StopListening()
	{
		s_listener?.Dispose();
		s_listener = null;
		s_activityListener?.Dispose();
		s_activityListener = null;
		s_includeDurations = false;
		s_includeHandlerDurations = false;
	}

	static void OnHandlerActivityStopped(Activity activity)
	{
		long durationNs = activity.Duration.Ticks * 100;
		string operation = activity.OperationName;

		s_handlerCountByOperation.TryGetValue(operation, out int operationCount);
		s_handlerCountByOperation[operation] = operationCount + 1;
		s_handlerNsByOperation.TryGetValue(operation, out long operationNs);
		s_handlerNsByOperation[operation] = operationNs + durationNs;

		if (!string.Equals(operation, "MapProperty", StringComparison.Ordinal) ||
			activity.GetTagItem("mapper.property") is not string property)
		{
			return;
		}

		s_mapperCountByProperty.TryGetValue(property, out int propertyCount);
		s_mapperCountByProperty[property] = propertyCount + 1;
		s_mapperNsByProperty.TryGetValue(property, out long propertyNs);
		s_mapperNsByProperty[property] = propertyNs + durationNs;
	}

	static void OnMeasurement(Instrument instrument, int value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
	{
		string name = instrument.Name;
		bool isMeasure = string.Equals(name, MeasureCounter, StringComparison.Ordinal);
		bool isArrange = string.Equals(name, ArrangeCounter, StringComparison.Ordinal);
		bool isMeasureDuration = string.Equals(name, MeasureHistogram, StringComparison.Ordinal);
		bool isArrangeDuration = string.Equals(name, ArrangeHistogram, StringComparison.Ordinal);

		if (isMeasure)
		{
			s_measureTotal += value;
		}
		else if (isArrange)
		{
			s_arrangeTotal += value;
		}
		else if (isMeasureDuration)
		{
			s_measureNsTotal += value;
		}
		else if (isArrangeDuration)
		{
			s_arrangeNsTotal += value;
		}

		string? elementType = null;
		for (int i = 0; i < tags.Length; i++)
		{
			if (string.Equals(tags[i].Key, "element.type", StringComparison.Ordinal))
			{
				elementType = tags[i].Value as string;
				break;
			}
		}

		if (elementType is null)
		{
			return;
		}

		if (isMeasureDuration)
		{
			s_measureNsByType.TryGetValue(elementType, out long currentNs);
			s_measureNsByType[elementType] = currentNs + value;
			return;
		}

		if (isArrangeDuration)
		{
			return;
		}

		var map = isMeasure ? s_measureByType : s_arrangeByType;
		map.TryGetValue(elementType, out int current);
		map[elementType] = current + value;
	}

	public static void Reset()
	{
		s_measureTotal = 0;
		s_arrangeTotal = 0;
		s_measureNsTotal = 0;
		s_arrangeNsTotal = 0;
		s_measureByType.Clear();
		s_arrangeByType.Clear();
		s_measureNsByType.Clear();
		s_handlerCountByOperation.Clear();
		s_handlerNsByOperation.Clear();
		s_mapperCountByProperty.Clear();
		s_mapperNsByProperty.Clear();
	}

	public static string TopHandlerPhases(int take) =>
		FormatDurations(s_handlerNsByOperation, s_handlerCountByOperation, take);

	public static string TopMapperProperties(int take) =>
		FormatDurations(s_mapperNsByProperty, s_mapperCountByProperty, take);

	static string FormatDurations(
		Dictionary<string, long> durations,
		Dictionary<string, int> counts,
		int take)
	{
		if (durations.Count == 0)
		{
			return "(none)";
		}

		var entries = new List<KeyValuePair<string, long>>(durations);
		entries.Sort(static (a, b) => b.Value.CompareTo(a.Value));

		var sb = new System.Text.StringBuilder();
		int limit = Math.Min(take, entries.Count);
		for (int i = 0; i < limit; i++)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			string name = entries[i].Key;
			counts.TryGetValue(name, out int count);
			sb.Append(name)
				.Append('=')
				.Append((entries[i].Value / 1_000_000.0).ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
				.Append("ms/")
				.Append(count);
		}

		return sb.ToString();
	}

	/// <summary>Returns the top <paramref name="take"/> element types by accumulated measure duration.</summary>
	public static string TopMeasureDurations(int take)
	{
		if (s_measureNsByType.Count == 0)
		{
			return "(none)";
		}

		var entries = new List<KeyValuePair<string, long>>(s_measureNsByType);
		entries.Sort(static (a, b) => b.Value.CompareTo(a.Value));

		var sb = new System.Text.StringBuilder();
		int limit = Math.Min(take, entries.Count);
		for (int i = 0; i < limit; i++)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			string name = entries[i].Key;
			int dot = name.LastIndexOf('.', StringComparison.Ordinal);
			sb.Append(dot >= 0 ? name.Substring(dot + 1) : name)
				.Append('=')
				.Append((entries[i].Value / 1_000_000.0).ToString("F1", System.Globalization.CultureInfo.InvariantCulture))
				.Append("ms");
		}

		return sb.ToString();
	}

	/// <summary>Returns the top <paramref name="take"/> element types by measure count, as "ShortName=count".</summary>
	public static string TopMeasureTypes(int take)
	{
		if (s_measureByType.Count == 0)
		{
			return "(none)";
		}

		var entries = new List<KeyValuePair<string, int>>(s_measureByType);
		entries.Sort(static (a, b) => b.Value.CompareTo(a.Value));

		var sb = new System.Text.StringBuilder();
		int limit = Math.Min(take, entries.Count);
		for (int i = 0; i < limit; i++)
		{
			if (i > 0)
			{
				sb.Append(", ");
			}

			string name = entries[i].Key;
			int dot = name.LastIndexOf('.', StringComparison.Ordinal);
			sb.Append(dot >= 0 ? name.Substring(dot + 1) : name).Append('=').Append(entries[i].Value);
		}

		return sb.ToString();
	}
}
