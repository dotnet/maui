using System.Diagnostics;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample;

/// <summary>
/// Drives the layout-performance scenario matrix and prints one machine-readable line per result.
///
/// Methodology notes:
///  * "ready" is defined exactly like the customer sample (Pages/MauiFeedPage.cs,
///    Popups/UxPopupPage.xaml.cs): after presentation, hop two dispatcher turns and stop the clock.
///    That is the same yardstick that produced their 500 ms / 354 ms / 260 ms numbers.
///  * Every scenario runs a timing pass (MeterListener detached, so counters are unsubscribed and cheap)
///    and a separate counting pass (MeterListener attached) so the counts never pollute the timings.
///  * Scrolling is driven with CollectionView.ScrollTo(index, animate: false) instead of Appium swipes so
///    the number of newly realized cells per step is deterministic and repeatable.
/// </summary>
public sealed class LayoutPerfRunner
{
	public const string LogPrefix = "SANDBOX-PERF";

	const int ScrollJumps = 8;
	const int ScrollStep = 6;
	const int SettleMs = 250;
	const int TimingReps = 7;

	readonly Page _owner;
	readonly Grid _overlayHost;
	readonly Action<string> _emit;
	readonly List<FeedItem> _items;
	readonly StringBuilder _summary = new();

	public LayoutPerfRunner(Page owner, Grid overlayHost, Action<string> emit, int itemCount)
	{
		_owner = owner;
		_overlayHost = overlayHost;
		_emit = emit;
		_items = FeedDataFactory.Create(itemCount);
	}

	public static PerfScenario[] Scenarios { get; } =
	{
		new()
		{
			Id = "A-popup-heavy-all",
			Name = "Modal popup, nested tile, MeasureAllItems",
			Host = PerfHost.Modal,
			Content = PerfContent.CollectionViewHeavy,
			Sizing = ItemSizingStrategy.MeasureAllItems,
		},
		new()
		{
			Id = "B-popup-heavy-first",
			Name = "Modal popup, nested tile, MeasureFirstItem",
			Host = PerfHost.Modal,
			Content = PerfContent.CollectionViewHeavy,
			Sizing = ItemSizingStrategy.MeasureFirstItem,
		},
		new()
		{
			Id = "C-page-heavy-all",
			Name = "Navigation page, nested tile, MeasureAllItems",
			Host = PerfHost.NavigationPush,
			Content = PerfContent.CollectionViewHeavy,
			Sizing = ItemSizingStrategy.MeasureAllItems,
		},
		new()
		{
			Id = "D-popup-flat-all",
			Name = "Modal popup, FLATTENED tile, MeasureAllItems",
			Host = PerfHost.Modal,
			Content = PerfContent.CollectionViewFlat,
			Sizing = ItemSizingStrategy.MeasureAllItems,
		},
		new()
		{
			Id = "E-popup-heavy-fixedh",
			Name = "Modal popup, nested tile + explicit height, MeasureAllItems",
			Host = PerfHost.Modal,
			Content = PerfContent.CollectionViewHeavyFixedHeight,
			Sizing = ItemSizingStrategy.MeasureAllItems,
		},
		new()
		{
			Id = "F-popup-stack30",
			Name = "Modal popup, ScrollView + VerticalStackLayout of 30 nested tiles (no virtualization)",
			Host = PerfHost.Modal,
			Content = PerfContent.NonVirtualizedStack,
			ItemCount = 30,
		},
		new()
		{
			Id = "G-overlay-heavy-all",
			Name = "In-page overlay, nested tile, MeasureAllItems",
			Host = PerfHost.InPageOverlay,
			Content = PerfContent.CollectionViewHeavy,
			Sizing = ItemSizingStrategy.MeasureAllItems,
		},
	};

	public string Summary => _summary.ToString();

	public async Task RunAllAsync(Action<string> status)
	{
		_summary.Clear();
		Emit("run-start", "items=" + _items.Count + " device=" + DeviceInfo.Model + " os=" + DeviceInfo.VersionString +
			" idiom=" + DeviceInfo.Idiom + " meterListening=" + LayoutMetrics.IsListening);

		ReportTileCost();
		ReportHandlerAttribution();
		ReportDiagnosticsProbe();

		for (int s = 0; s < Scenarios.Length; s++)
		{
			var scenario = Scenarios[s];
			var timings = new List<ScenarioTiming>(TimingReps);

			for (int rep = 1; rep <= TimingReps; rep++)
			{
				status($"{scenario.Id} timing rep {rep}/{TimingReps}");
				timings.Add(await RunScenarioAsync(scenario, rep, PerfPass.Time));
			}

			AppendSummary(scenario, timings);

			status($"{scenario.Id} counting pass");
			await RunScenarioAsync(scenario, TimingReps + 1, PerfPass.Count);

			status($"{scenario.Id} duration pass");
			await RunScenarioAsync(scenario, TimingReps + 2, PerfPass.Duration);

			status($"{scenario.Id} handler-attribution pass");
			await RunScenarioAsync(scenario, TimingReps + 3, PerfPass.Attribution);
		}

		Emit("run-end", "ok");
		status("done");
	}

	async Task<ScenarioTiming> RunScenarioAsync(PerfScenario scenario, int rep, PerfPass pass)
	{
		await QuiesceAsync();

		bool counting = pass != PerfPass.Time;

		if (counting)
		{
			LayoutMetrics.Reset();
			LayoutMetrics.StartListening(
				includeDurations: pass == PerfPass.Duration,
				includeHandlerDurations: pass == PerfPass.Attribution);
		}
		else
		{
			LayoutMetrics.StopListening();
		}

		// Build the object graph (XAML inflate + data binding setup, no platform views yet).
		long t = Stopwatch.GetTimestamp();
		var tree = PerfContentFactory.Build(scenario, _items);
		double buildMs = Ms(t);
		int buildMeasure = LayoutMetrics.MeasureTotal;

		var page = scenario.Host == PerfHost.InPageOverlay ? null : WrapInPage(tree.Root, scenario.Id);

		// First display: present and wait two dispatcher turns (customer's yardstick).
		// In parallel, arm the iOS CADisplayLink probe for a "frame actually committed" number.
		t = Stopwatch.GetTimestamp();
		long frameStart = t;
		var firstFrameCompletion = ScheduleFirstFrame(frameStart);
		await ShowAsync(scenario, page, tree.Root);
		double firstShowMs = Ms(t);
		double firstFrameMs = await ReadFirstFrameAsync(firstFrameCompletion);
		int showMeasure = LayoutMetrics.MeasureTotal - buildMeasure;
		int showArrange = LayoutMetrics.ArrangeTotal;

		await Task.Delay(SettleMs);
		int settleMeasure = LayoutMetrics.MeasureTotal - buildMeasure - showMeasure;

		// Scroll using deterministic jumps and measure the main-thread stall per jump.
		double scrollTotal = 0;
		double scrollWorst = 0;
		int scrollMeasureBefore = LayoutMetrics.MeasureTotal;
		var perJump = new StringBuilder();

		if (tree.List is not null)
		{
			for (int j = 1; j <= ScrollJumps; j++)
			{
				int index = Math.Min(j * ScrollStep, _items.Count - 1);
				long jt = Stopwatch.GetTimestamp();
				tree.List.ScrollTo(index, position: ScrollToPosition.Start, animate: false);
				await NextTurnsAsync(2);
				double jumpMs = Ms(jt);
				scrollTotal += jumpMs;
				scrollWorst = Math.Max(scrollWorst, jumpMs);

				if (j > 1)
				{
					perJump.Append(',');
				}

				perJump.Append(jumpMs.ToString("F1", System.Globalization.CultureInfo.InvariantCulture));
			}
		}

		int scrollMeasure = LayoutMetrics.MeasureTotal - scrollMeasureBefore;

		// Dismiss, then re-present the same instance (the customer's "preloaded" case).
		await HideAsync(scenario, page, tree.Root);
		await Task.Delay(SettleMs);

		int repeatBefore = LayoutMetrics.MeasureTotal;
		t = Stopwatch.GetTimestamp();
		long repeatFrameStart = t;
		var repeatFrameCompletion = ScheduleFirstFrame(repeatFrameStart);
		await ShowAsync(scenario, page, tree.Root);
		double repeatShowMs = Ms(t);
		double repeatFrameMs = await ReadFirstFrameAsync(repeatFrameCompletion);
		int repeatMeasure = LayoutMetrics.MeasureTotal - repeatBefore;

		await HideAsync(scenario, page, tree.Root);

		var line = new StringBuilder();
		line.Append(scenario.Id)
			.Append("|pass=").Append(PassName(pass))
			.Append("|rep=").Append(rep)
			.Append("|sizing=").Append(scenario.Sizing)
			.Append("|build_ms=").Append(F(buildMs))
			.Append("|first_show_ms=").Append(F(firstShowMs))
			.Append("|first_frame_ms=").Append(F(firstFrameMs))
			.Append("|repeat_show_ms=").Append(F(repeatShowMs))
			.Append("|repeat_frame_ms=").Append(F(repeatFrameMs))
			.Append("|scroll_total_ms=").Append(F(scrollTotal))
			.Append("|scroll_worst_ms=").Append(F(scrollWorst))
			.Append("|scroll_jumps_ms=").Append(perJump.Length == 0 ? "-" : perJump.ToString());

		if (counting)
		{
			line.Append("|build_measure=").Append(buildMeasure)
				.Append("|show_measure=").Append(showMeasure)
				.Append("|show_arrange=").Append(showArrange)
				.Append("|settle_measure=").Append(settleMeasure)
				.Append("|scroll_measure=").Append(scrollMeasure)
				.Append("|repeat_measure=").Append(repeatMeasure)
				.Append("|measure_total=").Append(LayoutMetrics.MeasureTotal)
				.Append("|arrange_total=").Append(LayoutMetrics.ArrangeTotal)
				.Append("|top_types=").Append(LayoutMetrics.TopMeasureTypes(20));

			if (pass == PerfPass.Duration)
			{
				line.Append("|measure_ns_total_ms=").Append(F(LayoutMetrics.MeasureNsTotal / 1_000_000.0))
					.Append("|arrange_ns_total_ms=").Append(F(LayoutMetrics.ArrangeNsTotal / 1_000_000.0))
					.Append("|top_durations=").Append(LayoutMetrics.TopMeasureDurations(20));
			}
			else if (pass == PerfPass.Attribution)
			{
				line.Append("|handler_ns_total_ms=").Append(F(LayoutMetrics.HandlerNsTotal / 1_000_000.0))
					.Append("|handler_phases=").Append(LayoutMetrics.TopHandlerPhases(20))
					.Append("|mapper_properties=").Append(LayoutMetrics.TopMapperProperties(30));
			}
		}

		Emit("result", line.ToString());

		if (counting)
		{
			LayoutMetrics.StopListening();
		}

		return new ScenarioTiming(firstShowMs, repeatShowMs, scrollWorst);
	}

	static Task<double>? ScheduleFirstFrame(long started)
	{
		var completion = new TaskCompletionSource<double>(TaskCreationOptions.RunContinuationsAsynchronously);
		return SandboxInstrumentation.ScheduleFirstFrame(() => completion.TrySetResult(Ms(started)))
			? completion.Task
			: null;
	}

	static async Task<double> ReadFirstFrameAsync(Task<double>? completion)
	{
		if (completion is null)
		{
			return -1;
		}

		try
		{
			return await completion.WaitAsync(TimeSpan.FromSeconds(1));
		}
		catch (TimeoutException)
		{
			return -1;
		}
	}

	void AppendSummary(PerfScenario scenario, List<ScenarioTiming> timings)
	{
		_summary.Append(scenario.Id)
			.Append(": show=").Append(F(Median(timings, static timing => timing.FirstShowMs)))
			.Append("ms repeat=").Append(F(Median(timings, static timing => timing.RepeatShowMs)))
			.Append("ms scrollWorst=").Append(F(Median(timings, static timing => timing.ScrollWorstMs)))
			.AppendLine("ms");
	}

	static double Median(List<ScenarioTiming> timings, Func<ScenarioTiming, double> selector)
	{
		var values = new double[timings.Count];
		for (int i = 0; i < timings.Count; i++)
		{
			values[i] = selector(timings[i]);
		}

		Array.Sort(values);
		return values[values.Length / 2];
	}

	readonly record struct ScenarioTiming(double FirstShowMs, double RepeatShowMs, double ScrollWorstMs);

	ContentPage WrapInPage(View content, string title)
	{
		var page = new ContentPage
		{
			Title = title,
			BackgroundColor = Color.FromArgb("#101828"),
			Content = content,
		};

		NavigationPage.SetHasNavigationBar(page, false);
		return page;
	}

	async Task ShowAsync(PerfScenario scenario, ContentPage? page, View content)
	{
		switch (scenario.Host)
		{
			case PerfHost.Modal:
				await _owner.Navigation.PushModalAsync(page!, false);
				break;

			case PerfHost.NavigationPush:
				await _owner.Navigation.PushAsync(page!, false);
				break;

			case PerfHost.InPageOverlay:
				_overlayHost.Clear();
				_overlayHost.Add(content);
				_overlayHost.IsVisible = true;
				break;
		}

		await NextTurnsAsync(2);
	}

	async Task HideAsync(PerfScenario scenario, ContentPage? page, View content)
	{
		switch (scenario.Host)
		{
			case PerfHost.Modal:
				await _owner.Navigation.PopModalAsync(false);
				break;

			case PerfHost.NavigationPush:
				await _owner.Navigation.PopAsync(false);
				break;

			case PerfHost.InPageOverlay:
				_overlayHost.IsVisible = false;
				_overlayHost.Clear();
				break;
		}

		await NextTurnsAsync(2);
	}

	/// <summary>
	/// Isolated per-tile cost breakdown, completely outside CollectionView: XAML inflate, platform view +
	/// handler creation (ToPlatform), first cross-platform Measure, an immediate second Measure with the
	/// same constraints (does the framework cache?), and Arrange.
	/// This is what separates "framework does too much per element" from "CollectionView does too much".
	/// </summary>
	void ReportTileCost()
	{
		var heavy = new HeavyFeedCardView { BindingContext = _items[0] };
		var flat = new FlatFeedCardView { BindingContext = _items[0] };

		Emit("tile", "heavy_elements=" + PerfContentFactory.CountVisualElements(heavy) +
			" flat_elements=" + PerfContentFactory.CountVisualElements(flat));

		ProbeTile("heavy", static () => new HeavyFeedCardView());
		ProbeTile("flat", static () => new FlatFeedCardView());

		ProbeSingleElement("Label", static () => new Label { Text = "Northwind Studio 0" });
		ProbeSingleElement("Border", static () => new Border { StrokeThickness = 1 });
		ProbeSingleElement("Grid", static () => new Grid());
		ProbeSingleElement("BoxView", static () => new BoxView());
		ProbeSingleElement("VerticalStackLayout", static () => new VerticalStackLayout());
	}

	/// <summary>
	/// Splits the per-element realization cost into "resolve the handler type from DI" and
	/// "create the platform view + run the whole property mapper" (ToPlatform minus resolution).
	/// </summary>
	void ProbeSingleElement(string label, Func<View> factory)
	{
		var mauiContext = _owner.Handler?.MauiContext;
		if (mauiContext is null)
		{
			return;
		}

		const int Warmup = 20;
		const int Iterations = 200;

		double resolveUs = 0;
		double toPlatformUs = 0;

		for (int i = 0; i < Warmup + Iterations; i++)
		{
			bool record = i >= Warmup;

			var probe = factory();

			long t = Stopwatch.GetTimestamp();
			var handler = mauiContext.Handlers.GetHandler(probe.GetType());
			double resolve = Ms(t) * 1000.0;

			var element = factory();
			t = Stopwatch.GetTimestamp();
			var platformView = element.ToPlatform(mauiContext);
			double toPlatform = Ms(t) * 1000.0;

			if (record)
			{
				resolveUs += resolve;
				toPlatformUs += toPlatform;
			}

			GC.KeepAlive(handler);
			GC.KeepAlive(platformView);
			element.DisconnectHandlers();
		}

		Emit("element-cost", label +
			" resolve_us=" + F3(resolveUs / Iterations) +
			" toplatform_us=" + F3(toPlatformUs / Iterations));
	}

	void ProbeTile(string label, Func<View> factory)
	{
		var mauiContext = _owner.Handler?.MauiContext;
		if (mauiContext is null)
		{
			Emit("tile-cost", label + " unavailable");
			return;
		}

		const int Warmup = 5;
		const int Iterations = 30;
		double width = _owner.Width > 0 ? _owner.Width : 393;

		double inflate = 0, toPlatform = 0, measure1 = 0, measure2 = 0, arrange = 0;
		int elements = 0;

		for (int i = 0; i < Warmup + Iterations; i++)
		{
			bool record = i >= Warmup;

			long t = Stopwatch.GetTimestamp();
			var tile = factory();
			double inflateMs = Ms(t);

			tile.BindingContext = _items[i % _items.Count];

			if (elements == 0)
			{
				elements = PerfContentFactory.CountVisualElements(tile);
			}

			t = Stopwatch.GetTimestamp();
			var platformView = tile.ToPlatform(mauiContext);
			double toPlatformMs = Ms(t);

			t = Stopwatch.GetTimestamp();
			var size = tile.Measure(width, double.PositiveInfinity);
			double measure1Ms = Ms(t);

			t = Stopwatch.GetTimestamp();
			tile.Measure(width, double.PositiveInfinity);
			double measure2Ms = Ms(t);

			t = Stopwatch.GetTimestamp();
			((IView)tile).Arrange(new Rect(0, 0, size.Width, size.Height));
			double arrangeMs = Ms(t);

			if (record)
			{
				inflate += inflateMs;
				toPlatform += toPlatformMs;
				measure1 += measure1Ms;
				measure2 += measure2Ms;
				arrange += arrangeMs;
			}

			GC.KeepAlive(platformView);
			tile.DisconnectHandlers();
		}

		Emit("tile-cost", label + " elements=" + elements +
			" inflate_ms=" + F3(inflate / Iterations) +
			" toplatform_ms=" + F3(toPlatform / Iterations) +
			" measure1_ms=" + F3(measure1 / Iterations) +
			" measure2_ms=" + F3(measure2 / Iterations) +
			" arrange_ms=" + F3(arrange / Iterations));
	}

	/// <summary>
	/// Attributes the per-tile handler realization cost to the individual phases of
	/// <c>ElementHandler.SetVirtualView</c> (CreatePlatformElement, AssignHandler, ConnectHandler,
	/// ResolveMapper, UpdateProperties) and to the individual mapper keys run by
	/// <c>PropertyMapper.UpdateProperties</c>.
	///
	/// The numbers come from the internal <c>Microsoft.Maui.Handlers</c> ActivitySource, so this pass is
	/// intrusive by construction and is reported separately from the clean wall-clock timings.
	/// Phase durations are Activity durations: <c>SetVirtualView</c> is the total and contains the others.
	/// </summary>
	void ReportHandlerAttribution()
	{
		var mauiContext = _owner.Handler?.MauiContext;
		if (mauiContext is null)
		{
			Emit("handler-attribution", "unavailable");
			return;
		}

		const int Warmup = 2;
		const int Iterations = 12;

		for (int i = 0; i < Warmup; i++)
		{
			var warmup = new HeavyFeedCardView { BindingContext = _items[i] };
			var platformView = warmup.ToPlatform(mauiContext);
			GC.KeepAlive(platformView);
			warmup.DisconnectHandlers();
		}

		LayoutMetrics.Reset();
		LayoutMetrics.StartListening(includeHandlerDurations: true);

		for (int i = 0; i < Iterations; i++)
		{
			var tile = new HeavyFeedCardView { BindingContext = _items[i] };
			var platformView = tile.ToPlatform(mauiContext);
			GC.KeepAlive(platformView);
			tile.DisconnectHandlers();
		}

		LayoutMetrics.StopListening();
		Emit("handler-attribution",
			"iterations=" + Iterations +
			" setvirtualview_ms=" + F3(LayoutMetrics.HandlerNsTotal / 1_000_000.0) +
			" phases=" + LayoutMetrics.TopHandlerPhases(20) +
			" mapper_properties=" + LayoutMetrics.TopMapperProperties(30));
	}

	/// <summary>
	/// Estimates the per-measure cost that MAUI's always-on diagnostics wrapper adds.
	/// <c>VisualElement.IView.Measure</c> calls <c>DiagnosticInstrumentation.StartLayoutMeasure</c>, which
	/// resolves <c>IDiagnosticsManager</c> from DI and builds a TagList on both the start and the stop side,
	/// for every element, on every measure — even when nothing is listening.
	/// This probe replays the same three operations with public API equivalents.
	/// </summary>
	void ReportDiagnosticsProbe()
	{
		var services = _owner.Handler?.MauiContext?.Services;
		if (services is null)
		{
			Emit("diag-probe", "unavailable");
			return;
		}

		const int N = 100_000;
		var probeSource = new ActivitySource("Sandbox.LayoutPerf.Probe", "1.0.0");
		object? sink = null;

		long t = Stopwatch.GetTimestamp();
		for (int i = 0; i < N; i++)
		{
			sink = services.GetService(typeof(IApplication));
		}

		double diNs = Ns(t, N);

		var view = (IView)_owner;
		int tagAccumulator = 0;
		t = Stopwatch.GetTimestamp();
		for (int i = 0; i < N; i++)
		{
			var tags = new System.Diagnostics.TagList();
			tags.Add("element.type", view.GetType().FullName);
			tagAccumulator += tags.Count;
		}

		double tagNs = Ns(t, N);

		t = Stopwatch.GetTimestamp();
		for (int i = 0; i < N; i++)
		{
			var activity = probeSource.StartActivity(ActivityKind.Internal, name: "Measure VisualElement");
			sink = activity;
		}

		double activityNs = Ns(t, N);

		GC.KeepAlive(sink);
		GC.KeepAlive(tagAccumulator);
		probeSource.Dispose();

		// Framework does: 2 x DI resolve + 2 x TagList build + 1 x StartActivity per Measure() call.
		double perMeasureNs = (2 * diNs) + (2 * tagNs) + activityNs;
		Emit("diag-probe", "di_ns=" + F(diNs) + " taglist_ns=" + F(tagNs) + " startactivity_ns=" + F(activityNs) +
			" est_per_measure_ns=" + F(perMeasureNs) +
			" est_per_10k_measures_ms=" + F(perMeasureNs * 10_000 / 1_000_000.0));
	}

	static async Task QuiesceAsync()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		await Task.Delay(SettleMs);
	}

	static Task NextTurnsAsync(int turns)
	{
		var dispatcher = Application.Current?.Dispatcher;
		if (dispatcher is null)
		{
			return Task.CompletedTask;
		}

		var tcs = new TaskCompletionSource();
		int remaining = turns;

		void Hop()
		{
			if (--remaining <= 0)
			{
				tcs.TrySetResult();
			}
			else
			{
				dispatcher.Dispatch(Hop);
			}
		}

		dispatcher.Dispatch(Hop);
		return tcs.Task;
	}

	static string PassName(PerfPass pass) => pass switch
	{
		PerfPass.Count => "count",
		PerfPass.Duration => "duration",
		PerfPass.Attribution => "attribution",
		_ => "time",
	};

	static double Ms(long startTimestamp) =>
		(Stopwatch.GetTimestamp() - startTimestamp) * 1000.0 / Stopwatch.Frequency;

	static double Ns(long startTimestamp, int iterations) =>
		(Stopwatch.GetTimestamp() - startTimestamp) * 1_000_000_000.0 / Stopwatch.Frequency / iterations;

	static string F(double value) => value.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);

	static string F3(double value) => value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);

	void Emit(string kind, string payload)
	{
		string line = LogPrefix + "|" + kind + "|" + payload;
		Console.WriteLine(line);
		Debug.WriteLine(line);
		_emit(line);
	}
}
