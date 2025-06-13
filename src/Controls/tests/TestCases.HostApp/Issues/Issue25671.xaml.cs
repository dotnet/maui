using System.Text;
using System.Text.RegularExpressions;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25671, "Layout passes should not increase", PlatformAffected.All)]

public partial class Issue25671 : ContentPage
{
	private int _regenIndex = 2;
	private static readonly string _loremIpsumLongText = @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed non risus. Suspendisse lectus tortor, dignissim sit amet, adipiscing nec, ultricies sed, dolor. Cras elementum ultrices diam. Maecenas ligula massa, varius a, semper congue, euismod non, mi. Proin porttitor, orci nec nonummy molestie, enim est eleifend mi, non fermentum diam nisl sit amet erat. Duis semper. Duis arcu massa, scelerisque vitae, consequat in, pretium a, enim. Pellentesque congue.";
	private static readonly string[] _words = new Regex(@"\w+").Matches(_loremIpsumLongText).Select(m => m.Value).ToArray();
	private static readonly Queue<string> _statsLines = new();
	private readonly VisualElementProfiler _profiler;

	public Issue25671()
	{
		InitializeComponent();
		GenerateItems();
		_profiler = new VisualElementProfiler();
		_profiler.Attach(this);

		var statsTap = new TapGestureRecognizer();
		statsTap.Tapped += (s, e) =>
		{
			NextStatsLine();
		};
		Stats.GestureRecognizers.Add(statsTap);

		CV.HandlerChanged += (s, e) =>
		{
			if (CV.Handler is { } handler)
			{
				HeadingLabel.Text = handler.GetType().Name;
			}
		};
	}

	void NextStatsLine()
	{
		Stats.Text = _statsLines.TryDequeue(out var line) ? line : string.Empty;
	}

	void RegenerateItems(object sender, EventArgs args)
	{
		_regenIndex = (_regenIndex - 1) % 4 + 2;
		GenerateItems();
	}

	void OnClick(object sender, EventArgs args)
	{
		Console.Write($"\n{_profiler}");

		long measurePasses = 0;
		long crossPlatformMeasurePasses = 0;
		long arrangePasses = 0;
		long crossPlatformArrangePasses = 0;
		foreach (var (_, stats) in _profiler.TypeStats)
		{
			measurePasses += stats[LayoutPassType.Measure].StandaloneCount;
			crossPlatformMeasurePasses += stats[LayoutPassType.CrossPlatformMeasure].StandaloneCount;
			arrangePasses += stats[LayoutPassType.Arrange].StandaloneCount;
			crossPlatformArrangePasses += stats[LayoutPassType.CrossPlatformArrange].StandaloneCount;
		}

		_statsLines.Clear();
		var lines = _profiler.ToString().Split("\n");
		foreach (var line in lines)
		{
			_statsLines.Enqueue(line);
		}

		NextStatsLine(); 
		((Button)sender).Text = $"M: {measurePasses}, A: {arrangePasses}, CPM: {crossPlatformMeasurePasses}, CPA: {crossPlatformArrangePasses}";
	}

	void GenerateItems()
	{
		CV.ItemsSource = Enumerable.Range(4, 200).Select(i =>
			new
			{
				Text = string.Join(' ', _words.Take(i % _regenIndex == 0 ? i % _words.Length : (i * 2) % _words.Length)),
				Glyph = char.ConvertFromUtf32(0xf127 + i % 10)
			});
	}
}
