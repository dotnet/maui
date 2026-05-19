using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35492, "Border.StrokeDashArray leaks dashed Borders when using a shared Application resource", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue35492 : NavigationPage
{
	const string SharedDashArrayResourceKey = "SharedDashArray";

	public Issue35492() : base(new Issue35492DashboardPage())
	{
		if (Application.Current?.Resources is not null && !Application.Current.Resources.ContainsKey(SharedDashArrayResourceKey))
			Application.Current.Resources.Add(SharedDashArrayResourceKey, new DoubleCollection(new[] { 6d, 3d, 1d, 3d }));
	}

	internal static DoubleCollection GetSharedDashArray()
	{
		return Application.Current?.Resources?[SharedDashArrayResourceKey] as DoubleCollection;
	}
}

enum Issue35492ReproMode
{
	SharedAppResourceDashArray,
	SolidBorderControl,
	PerBorderDashArrayMitigation
}

sealed record Issue35492ReproOptions(
	Issue35492ReproMode Mode,
	int Pages,
	int CardsPerPage,
	int ItemPayloadKilobytes,
	int PagePayloadMegabytes,
	int DwellMilliseconds)
{
	public bool UseSharedDashArray => Mode == Issue35492ReproMode.SharedAppResourceDashArray;
	public bool UsePerBorderDashArray => Mode == Issue35492ReproMode.PerBorderDashArrayMitigation;
	public long ItemPayloadBytes => ItemPayloadKilobytes * 1024L;
	public long PagePayloadBytes => PagePayloadMegabytes * 1024L * 1024L;
	public long PayloadBytesPerPage => PagePayloadBytes + CardsPerPage * ItemPayloadBytes;
	public string Name => Mode switch
	{
		Issue35492ReproMode.SharedAppResourceDashArray => "leaky shared AppResource StrokeDashArray",
		Issue35492ReproMode.SolidBorderControl => "control: solid borders",
		Issue35492ReproMode.PerBorderDashArrayMitigation => "mitigation: per-border dash arrays",
		_ => Mode.ToString()
	};
}

sealed class Issue35492DashboardPage : ContentPage
{
	static readonly Issue35492ReproOptions LeakRunOptions = new(
		Issue35492ReproMode.SharedAppResourceDashArray,
		1,
		64,
		96,
		3,
		100);

	readonly Button _runLeakButton;
	readonly ProgressBar _progress;
	readonly Label _statusLabel;
	readonly Label _summaryLabel;
	CancellationTokenSource _runCancellation;
	Issue35492MemorySnapshot _baseline = Issue35492MemorySnapshot.Empty;

	public Issue35492DashboardPage()
	{
		Title = "Issue 35492";
		BackgroundColor = Colors.White;

		_runLeakButton = CreateButton("Run shared resource leak", "RunSharedResourceLeakButton", RunAsync);

		_progress = new ProgressBar
		{
			Progress = 0,
			HeightRequest = 6,
			ProgressColor = Color.FromArgb("#2563EB"),
			AutomationId = "ProgressBar"
		};

		_statusLabel = new Label
		{
			Text = "Ready. Run shared resource scenario.",
			TextColor = Color.FromArgb("#1E293B"),
			FontSize = 14,
			AutomationId = "StatusLabel"
		};

		_summaryLabel = new Label
		{
			Text = "Alive count: 0/0",
			TextColor = Color.FromArgb("#1E293B"),
			FontFamily = GetMonospaceFontFamily(),
			FontSize = 13,
			LineBreakMode = LineBreakMode.WordWrap,
			AutomationId = "SummaryLabel"
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(18, 18, 18, 28),
				Spacing = 16,
				Children =
				{
					new Label
					{
						Text = "Border StrokeDashArray retention",
						FontSize = 22,
						FontAttributes = FontAttributes.Bold,
						TextColor = Color.FromArgb("#0F172A"),
						AutomationId = "TitleLabel"
					},
					new Label
					{
						Text = "A shared dashed border resource is a normal app pattern. This repro shows how it can retain realized card Borders and, through page-level event handlers, the whole page, CollectionView, and view models.",
						FontSize = 14,
						TextColor = Color.FromArgb("#475569"),
						AutomationId = "DescriptionLabel"
					},
					_runLeakButton,
					_progress,
					_statusLabel,
					_summaryLabel
				}
			}
		};
	}

	static Button CreateButton(string text, string automationId, Func<Task> action)
	{
		var button = new Button
		{
			Text = text,
			FontSize = 14,
			BackgroundColor = Color.FromArgb("#2563EB"),
			TextColor = Colors.White,
			CornerRadius = 6,
			MinimumHeightRequest = 44,
			AutomationId = automationId
		};

		button.Clicked += async (_, _) => await action();
		return button;
	}

	async Task RunAsync()
	{
		if (_runCancellation is not null)
			return;

		var options = LeakRunOptions;
		var session = new Issue35492ReproSession(options);

		_runCancellation = new CancellationTokenSource();
		var token = _runCancellation.Token;

		SetRunning(true);
		_progress.Progress = 0;
		_summaryLabel.Text = "Alive count: collecting...";

		try
		{
			_baseline = await Issue35492MemorySampler.TakeAfterCollectionAsync();
			_summaryLabel.Text = "Alive count: running...";

			for (var i = 0; i < options.Pages; i++)
			{
				token.ThrowIfCancellationRequested();
				var cycle = session.BeginNextCycle();
				_statusLabel.Text = $"Pushing CollectionView page {cycle + 1}/{options.Pages}: {options.Name}";

				await Navigation.PushAsync(new Issue35492ReproPage(session), false);

				if (options.DwellMilliseconds > 0)
					await Task.Delay(options.DwellMilliseconds, token);

				_progress.Progress = (i + 1d) / (options.Pages * 2d);
			}

			for (var i = 0; i < options.Pages; i++)
			{
				token.ThrowIfCancellationRequested();
				_statusLabel.Text = $"Popping CollectionView page {i + 1}/{options.Pages}: {options.Name}";

				await Navigation.PopAsync(false);
				await Task.Delay(25, token);

				if ((i + 1) % 5 == 0 || i + 1 == options.Pages)
				{
					var current = await Issue35492MemorySampler.TakeAfterCollectionAsync();
					_summaryLabel.Text = session.GetStats(_baseline, current).ToPrimaryMetric();
				}

				_progress.Progress = (options.Pages + i + 1d) / (options.Pages * 2d);
			}

			var finalSnapshot = await Issue35492MemorySampler.TakeAfterCollectionAsync();
			_summaryLabel.Text = session.GetStats(_baseline, finalSnapshot).ToPrimaryMetric();
			_statusLabel.Text = $"Completed {options.Name}.";
		}
		catch (OperationCanceledException)
		{
			_statusLabel.Text = "Run stopped.";
		}
		catch (Exception ex)
		{
			_statusLabel.Text = "Run failed.";
			_summaryLabel.Text = ex.ToString();
		}
		finally
		{
			_runCancellation?.Dispose();
			_runCancellation = null;
			SetRunning(false);
		}
	}

	Task StopRun()
	{
		_runCancellation?.Cancel();
		return Task.CompletedTask;
	}

	void SetRunning(bool isRunning)
	{
		_runLeakButton.IsEnabled = !isRunning;
	}

	static string GetMonospaceFontFamily()
	{
#if IOS || MACCATALYST
		return "Menlo";
#else
		return null;
#endif
	}
}

sealed class Issue35492ReproPage : ContentPage
{
	readonly CollectionView _collectionView;
	readonly Issue35492PagePayloadViewModel _payload;

	public Issue35492ReproPage(Issue35492ReproSession session)
	{
		var options = session.Options;
		var cycle = session.CurrentCycle;

		_payload = new Issue35492PagePayloadViewModel(cycle, options);
		Title = _payload.Title;
		BindingContext = _payload;
		BackgroundColor = Color.FromArgb("#F8FAFC");

		_collectionView = new CollectionView
		{
			ItemsSource = _payload.Cards,
			ItemsLayout = new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)
			{
				HorizontalItemSpacing = 12,
				VerticalItemSpacing = 12
			},
			ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem,
			ItemTemplate = new DataTemplate(() => CreateCardView(options, session)),
			Header = CreateHeader(options),
			Footer = new BoxView { HeightRequest = 24, Opacity = 0 },
			Margin = new Thickness(14, 0),
			AutomationId = "TestCollectionView"
		};

		session.TrackPage(this, _collectionView, _payload);
		Content = _collectionView;
	}

	View CreateHeader(Issue35492ReproOptions options)
	{
		return new VerticalStackLayout
		{
			Padding = new Thickness(0, 18, 0, 14),
			Spacing = 8,
			Children =
			{
				new Label
				{
					Text = _payload.Title,
					FontSize = 22,
					FontAttributes = FontAttributes.Bold,
					TextColor = Color.FromArgb("#0F172A")
				},
				new Label
				{
					Text = $"{options.Name}: {_payload.Cards.Count} account cards, {Issue35492ReproStats.FormatBytes(options.PayloadBytesPerPage)} simulated page/item payload.",
					FontSize = 13,
					TextColor = Color.FromArgb("#475569")
				}
			}
		};
	}

	View CreateCardView(Issue35492ReproOptions options, Issue35492ReproSession session)
	{
		var title = new Label { FontSize = 15, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#0F172A"), LineBreakMode = LineBreakMode.TailTruncation };
		title.SetBinding(Label.TextProperty, nameof(Issue35492CardPayloadViewModel.Title));

		var status = new Label { FontSize = 12, TextColor = Color.FromArgb("#2563EB"), LineBreakMode = LineBreakMode.TailTruncation };
		status.SetBinding(Label.TextProperty, nameof(Issue35492CardPayloadViewModel.Status));

		var owner = new Label { FontSize = 12, TextColor = Color.FromArgb("#64748B"), LineBreakMode = LineBreakMode.TailTruncation };
		owner.SetBinding(Label.TextProperty, nameof(Issue35492CardPayloadViewModel.Owner), stringFormat: "Owner: {0}");

		var amount = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Color.FromArgb("#14532D"), HorizontalTextAlignment = TextAlignment.End };
		amount.SetBinding(Label.TextProperty, nameof(Issue35492CardPayloadViewModel.AmountText));

		var body = new Grid
		{
			ColumnDefinitions = { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Auto) },
			RowDefinitions = { new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto) },
			ColumnSpacing = 10,
			RowSpacing = 4
		};

		body.Add(title, 0, 0);
		body.Add(status, 0, 1);
		body.Add(owner, 0, 2);
		body.Add(amount, 1, 0);
		Grid.SetRowSpan(amount, 3);

		var border = new Border
		{
			Stroke = Color.FromArgb("#64748B"),
			StrokeThickness = 1.5,
			StrokeShape = new RoundRectangle { CornerRadius = 6 },
			Padding = new Thickness(12),
			BackgroundColor = Colors.White,
			HeightRequest = 106,
			MinimumHeightRequest = 106,
			Content = body
		};

		if (options.UseSharedDashArray)
			border.StrokeDashArray = Issue35492.GetSharedDashArray();
		else if (options.UsePerBorderDashArray)
			border.StrokeDashArray = new DoubleCollection(new[] { 6d, 3d, 1d, 3d });

		var tapGesture = new TapGestureRecognizer();
		tapGesture.Tapped += (_, _) =>
		{
			if (border.BindingContext is Issue35492CardPayloadViewModel card)
				_payload.OpenCardCommand.Execute(card);
		};
		border.GestureRecognizers.Add(tapGesture);

		var tracked = false;
		border.BindingContextChanged += (_, _) =>
		{
			if (!tracked && border.BindingContext is Issue35492CardPayloadViewModel card)
			{
				tracked = true;
				session.TrackCardBorder(border, card);
			}
		};

		return border;
	}
}

sealed class Issue35492ReproSession
{
	readonly List<Issue35492TrackedPage> _trackedPages = new();
	readonly List<Issue35492TrackedCard> _trackedCards = new();
	readonly Stopwatch _elapsed = Stopwatch.StartNew();
	int _currentCycle = -1;

	public Issue35492ReproSession(Issue35492ReproOptions options)
	{
		Options = options;
	}

	public Issue35492ReproOptions Options { get; }

	public int CurrentCycle => _currentCycle;

	public int BeginNextCycle() => ++_currentCycle;

	public void TrackPage(ContentPage page, CollectionView collectionView, Issue35492PagePayloadViewModel payload)
	{
		_trackedPages.Add(new Issue35492TrackedPage(
			new WeakReference(page),
			new WeakReference(collectionView),
			new WeakReference(payload)));

		foreach (var card in payload.Cards)
			TrackCardPayload(card);
	}

	public void TrackCardBorder(Border border, Issue35492CardPayloadViewModel payload)
	{
		_trackedCards.Add(new Issue35492TrackedCard(
			new WeakReference(border),
			new WeakReference(payload),
			true));
	}

	void TrackCardPayload(Issue35492CardPayloadViewModel payload)
	{
		_trackedCards.Add(new Issue35492TrackedCard(
			null,
			new WeakReference(payload),
			false));
	}

	public Issue35492ReproStats GetStats(Issue35492MemorySnapshot baseline, Issue35492MemorySnapshot current)
	{
		var alivePages = 0;
		var aliveCollectionViews = 0;
		var alivePageViewModels = 0;
		var aliveCardViewModels = 0;
		var aliveCardBorders = 0;
		var trackedCardBorders = 0;

		foreach (var page in _trackedPages)
		{
			if (page.Page.IsAlive)
				alivePages++;

			if (page.CollectionView.IsAlive)
				aliveCollectionViews++;

			if (page.ViewModel.IsAlive)
				alivePageViewModels++;
		}

		foreach (var card in _trackedCards)
		{
			if (card.TracksBorder)
			{
				trackedCardBorders++;

				if (card.Border?.IsAlive == true)
					aliveCardBorders++;
			}
			else if (card.Payload.IsAlive)
			{
				aliveCardViewModels++;
			}
		}

		var retainedPayloadBytes = alivePages * Options.PayloadBytesPerPage;

		return new Issue35492ReproStats(
			Options,
			_trackedPages.Count,
			trackedCardBorders,
			alivePages,
			aliveCollectionViews,
			alivePageViewModels,
			aliveCardViewModels,
			aliveCardBorders,
			retainedPayloadBytes,
			baseline,
			current,
			_elapsed.Elapsed);
	}
}

sealed record Issue35492TrackedPage(WeakReference Page, WeakReference CollectionView, WeakReference ViewModel);

sealed record Issue35492TrackedCard(WeakReference Border, WeakReference Payload, bool TracksBorder);

sealed class Issue35492PagePayloadViewModel
{
	public Issue35492PagePayloadViewModel(int cycle, Issue35492ReproOptions options)
	{
		Cycle = cycle;
		PayloadBytes = options.PagePayloadBytes;
		Payload = CreatePayload(PayloadBytes, cycle);
		OpenCardCommand = new Command<Issue35492CardPayloadViewModel>(_ => Taps++);

		for (var i = 0; i < options.CardsPerPage; i++)
			Cards.Add(new Issue35492CardPayloadViewModel(cycle, i, options.ItemPayloadBytes));
	}

	public int Cycle { get; }
	public long PayloadBytes { get; }
	public byte[] Payload { get; }
	public ObservableCollection<Issue35492CardPayloadViewModel> Cards { get; } = new();
	public ICommand OpenCardCommand { get; }
	public int Taps { get; private set; }
	public string Title => $"Customer portfolio {Cycle + 1}";

	static byte[] CreatePayload(long payloadBytes, int salt)
	{
		var payload = new byte[payloadBytes];
		for (var i = 0; i < payload.Length; i += 4096)
			payload[i] = (byte)(salt + i);
		return payload;
	}
}

sealed class Issue35492CardPayloadViewModel
{
	public Issue35492CardPayloadViewModel(int pageCycle, int index, long payloadBytes)
	{
		PageCycle = pageCycle;
		Index = index;
		PayloadBytes = payloadBytes;
		Payload = CreatePayload(payloadBytes, pageCycle, index);
		Status = Statuses[(pageCycle + index) % Statuses.Length];
		Owner = Owners[(pageCycle * 7 + index) % Owners.Length];
		Amount = 25000 + (pageCycle * 8191 + index * 977) % 950000;
	}

	static readonly string[] Statuses = ["Pending review", "Escalated", "Awaiting signature", "In underwriting"];
	static readonly string[] Owners = ["Avery Stone", "Jordan Lee", "Sam Rivera", "Morgan Patel", "Taylor Kim"];

	public int PageCycle { get; }
	public int Index { get; }
	public long PayloadBytes { get; }
	public byte[] Payload { get; }
	public string Title => $"Account {PageCycle + 1:00}-{Index + 1:000}";
	public string Status { get; }
	public string Owner { get; }
	public int Amount { get; }
	public string AmountText => $"${Amount:N0}";

	static byte[] CreatePayload(long payloadBytes, int pageCycle, int index)
	{
		var payload = new byte[payloadBytes];
		for (var i = 0; i < payload.Length; i += 4096)
			payload[i] = (byte)(pageCycle + index + i);
		return payload;
	}
}

sealed record Issue35492ReproStats(
	Issue35492ReproOptions Options,
	int TrackedPages,
	int TrackedCardBorders,
	int AlivePages,
	int AliveCollectionViews,
	int AlivePageViewModels,
	int AliveCardViewModels,
	int AliveCardBorders,
	long RetainedPayloadBytes,
	Issue35492MemorySnapshot Baseline,
	Issue35492MemorySnapshot Current,
	TimeSpan Elapsed)
{
	public string ToSummary()
	{
		var expectedPayload = Options.PayloadBytesPerPage * TrackedPages;
		var retainedPercent = expectedPayload == 0 ? 0 : RetainedPayloadBytes * 100.0 / expectedPayload;

		return string.Join(Environment.NewLine,
			$"Run: {Options.Name}",
			$"Pages pushed and popped: {TrackedPages} in {Elapsed:mm\\:ss}",
			$"Weak refs still alive after full GC:",
			$"  pages: {AlivePages}/{TrackedPages}",
			$"  CollectionViews: {AliveCollectionViews}/{TrackedPages}",
			$"  page view models: {AlivePageViewModels}/{TrackedPages}",
			$"  card view models: {AliveCardViewModels}/{TrackedPages * Options.CardsPerPage}",
			$"  realized card Borders: {AliveCardBorders}/{TrackedCardBorders}",
			$"Payload definitely retained through alive pages: {FormatBytes(RetainedPayloadBytes)} ({retainedPercent:0.0}% of allocated payload)",
			$"Managed heap delta after GC: {FormatBytes(Current.ManagedBytes - Baseline.ManagedBytes)}",
			$"GC heap delta after GC: {FormatBytes(Current.GcHeapBytes - Baseline.GcHeapBytes)}",
			$"Resident memory delta: {FormatBytes(Current.ResidentBytes - Baseline.ResidentBytes)}",
			$"Working set delta: {FormatBytes(Current.WorkingSetBytes - Baseline.WorkingSetBytes)}");
	}

	public string ToPrimaryMetric()
	{
		return $"Alive count: {AlivePages}/{TrackedPages}";
	}

	public static string FormatBytes(long bytes)
	{
		var sign = bytes < 0 ? "-" : string.Empty;
		var value = Math.Abs(bytes);

		if (value >= 1024L * 1024L * 1024L)
			return $"{sign}{value / 1024d / 1024d / 1024d:0.0} GB";
		if (value >= 1024L * 1024L)
			return $"{sign}{value / 1024d / 1024d:0.0} MB";
		if (value >= 1024L)
			return $"{sign}{value / 1024d:0.0} KB";
		return $"{sign}{value} B";
	}
}

sealed record Issue35492MemorySnapshot(long ManagedBytes, long GcHeapBytes, long ResidentBytes, long WorkingSetBytes)
{
	public static Issue35492MemorySnapshot Empty { get; } = new(0, 0, 0, 0);
}

static class Issue35492MemorySampler
{
	public static async Task ForceFullCollectionAsync()
	{
		await Task.Yield();

		for (var i = 0; i < 3; i++)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
#if ANDROID
			Java.Lang.JavaSystem.Gc();
			Java.Lang.Runtime.GetRuntime()?.Gc();
#endif
			await Task.Delay(25);
		}
	}

	public static async Task<Issue35492MemorySnapshot> TakeAfterCollectionAsync()
	{
		await ForceFullCollectionAsync();
		return new Issue35492MemorySnapshot(
			GC.GetTotalMemory(forceFullCollection: false),
			GC.GetGCMemoryInfo().HeapSizeBytes,
			GetResidentMemoryBytes(),
			GetWorkingSetBytes());
	}

	static long GetWorkingSetBytes()
	{
		try
		{
			return Process.GetCurrentProcess().WorkingSet64;
		}
		catch
		{
			return 0;
		}
	}

	static long GetResidentMemoryBytes()
	{
#if IOS || MACCATALYST
		try
		{
			var info = new MachTaskBasicInfo();
			var count = (uint)(Marshal.SizeOf<MachTaskBasicInfo>() / sizeof(int));
			if (task_info(mach_task_self(), MachTaskBasicInfoFlavor, out info, ref count) == 0)
				return info.ResidentSize.ToInt64();
		}
		catch
		{
		}
#endif
		return GetWorkingSetBytes();
	}

#if IOS || MACCATALYST
	const int MachTaskBasicInfoFlavor = 20;

	[DllImport("/usr/lib/libSystem.dylib")]
	static extern IntPtr mach_task_self();

	[DllImport("/usr/lib/libSystem.dylib")]
	static extern int task_info(IntPtr targetTask, int flavor, out MachTaskBasicInfo taskInfo, ref uint taskInfoCount);

	[StructLayout(LayoutKind.Sequential)]
	struct TimeValue
	{
		public int Seconds;
		public int Microseconds;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct MachTaskBasicInfo
	{
		public IntPtr VirtualSize;
		public IntPtr ResidentSize;
		public IntPtr ResidentSizeMax;
		public TimeValue UserTime;
		public TimeValue SystemTime;
		public int Policy;
		public int SuspendCount;
	}
#endif
}
