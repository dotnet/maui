using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Controls.Animations;

namespace Maui.Controls.Sample;

// A festival-themed music player that shows the prototype animation engine in a believable app. It plays a scripted
// show once on launch, so it can be recorded hands-free, and stays fully tappable afterwards.
public partial class MainStagePage : ContentPage
{
	static readonly (string Title, string Artist)[] Sets =
	{
		("GoToState of Mind", "Gerald & The Handlers"),
		("Lerp of Faith", "Hot Reload"),
		("Keyframe of Mind", "XAML Grooves"),
	};

	readonly Stopwatch _clock = Stopwatch.StartNew();
	readonly Border[] _acts;
	StoryboardPlayer? _ledWall;
	int _selected = -1;
	bool _liked;
	bool _backstageOpen;
	bool _showStarted;
	TimeSpan _showStart;

	public MainStagePage()
	{
		InitializeComponent();
		_acts = new[] { Act0, Act1, Act2 };

		// Initial states apply instantly: a group with no current state never animates.
		foreach (var act in _acts)
			VisualStateManager.GoToState(act, "Normal");
		VisualStateManager.GoToState(LikeButton, "NotLiked");

		// PosterIn starts on Root's Loaded (PlayOnLoaded); the show's timetable starts on the same event, so the entrance and
		// every later beat share one clock. A page's Loaded can arrive a few hundred milliseconds later than its content's.
		Root.Loaded += OnLoaded;
		Loaded += (_, _) => Log("STEP", "page-loaded");
	}

	async void OnLoaded(object? sender, EventArgs e)
	{
		if (_showStarted)
			return;

		_showStarted = true;
		_showStart = _clock.Elapsed;
		Step("loaded");
		await RunShowAsync();
	}

	// The show keeps a fixed timetable from page load, so recordings on different devices stay in step.
	Task At(double seconds)
	{
		var wait = _showStart + TimeSpan.FromSeconds(seconds) - _clock.Elapsed;
		return wait > TimeSpan.Zero ? Task.Delay(wait) : Task.CompletedTask;
	}

	async Task RunShowAsync()
	{
		// PosterIn waits 0.6 s for the launch screen to clear, then plays for 1.4 s.
		await At(2.0);
		SetPlaying(true);

		await At(3.5);
		Step("select");
		Select(0);

		await At(5.1);
		Step("interrupt");
		Select(1);
		await At(5.36);
		Select(2);
		await At(5.62);
		Select(0);

		await At(8.0);
		Step("pause");
		await PressPlayAsync();

		await At(9.6);
		Step("play");
		await PressPlayAsync();

		await At(11.2);
		Step("like");
		await ToggleLikeAsync();

		await At(14.2);
		Step("skip");
		_ = SkipAsync();

		await At(17.2);
		Step("backstage");
		await RunBackstageAsync();

		await At(25.2);
		Step("done");
		await CloseBackstageAsync();
	}

	void Select(int index)
	{
		var notifications = 0;
		void Count(object? s, System.ComponentModel.PropertyChangedEventArgs args)
		{
			if (args.PropertyName == nameof(Scale))
				notifications++;
		}

		var act = _acts[index];
		act.PropertyChanged += Count;
		for (var i = 0; i < _acts.Length; i++)
			VisualStateManager.GoToState(_acts[i], i == index ? "Selected" : "Normal");
		act.PropertyChanged -= Count;

		// The state changes synchronously; the pinned properties raise nothing at the switch.
		Log("SWITCH", $"act {index}; Scale notifications during GoToState: {notifications}");

		if (index != _selected)
		{
			_selected = index;
			TrackTitle.Text = Sets[index].Title;
			TrackArtist.Text = Sets[index].Artist;
			_ = Board("SwapIn").PlayAsync(NowPlaying);
		}
	}

	async Task PressPlayAsync()
	{
		// What a tap does: Pressed, then Normal. Both state changes animate with the transitions in XAML.
		VisualStateManager.GoToState(PlayButton, "Pressed");
		await Task.Delay(170);
		VisualStateManager.GoToState(PlayButton, "Normal");
		SetPlaying(_ledWall is null);
	}

	void SetPlaying(bool playing)
	{
		if (playing)
		{
			_ledWall ??= Board("LedWall").Begin(this);
			PlayGlyph.Text = "❚❚";
			PlayGlyph.FontSize = 22;
			PlayGlyph.TranslationX = 0;
			SemanticProperties.SetDescription(PlayButton, "Pause");
			LiveDot.Opacity = 1;
			LiveLabel.Text = "LIVE · MAIN STAGE";
		}
		else
		{
			// Stop() releases every bar: each one drops back to its resting height.
			_ledWall?.Stop();
			_ledWall = null;
			// The triangle glyph reads small and sits left of center; size it up and nudge it right.
			PlayGlyph.Text = "►";
			PlayGlyph.FontSize = 30;
			PlayGlyph.TranslationX = 3;
			SemanticProperties.SetDescription(PlayButton, "Play");
			LiveDot.Opacity = 0.45;
			LiveLabel.Text = "PAUSED";
		}

		Log("STATE", playing ? "playing" : "paused");
	}

	async Task ToggleLikeAsync()
	{
		_liked = !_liked;
		VisualStateManager.GoToState(LikeButton, _liked ? "Liked" : "NotLiked");
		if (!_liked)
			return;

		var result = await Board("StampLike").PlayAsync(this);
		Log("RESULT", $"StampLike -> {result}");
	}

	async Task SkipAsync()
	{
		var shake = Board("Shake").PlayAsync(this);
		var toast = Board("Toast").PlayAsync(this);
		await Task.WhenAll(shake, toast);
		Log("RESULT", $"Shake -> {shake.Result}, Toast -> {toast.Result}");
	}

	async Task RunBackstageAsync()
	{
		if (_backstageOpen)
			return;

		_backstageOpen = true;
		DimmerSlider.Value = 1;
		// Rebinding demotes FadeToAsync's leftover manual value, so every visit starts fresh.
		LegacyLight.SetBinding(OpacityProperty, new Binding(nameof(Slider.Value), source: DimmerSlider));
		LegacyResult.Opacity = 0;
		NewResult.Opacity = 0;
		BackstageHost.InputTransparent = false;
		await Board("BackstageIn").PlayAsync(this);
		await Task.Delay(1000);

		Step("fade");
		var legacy = LegacyLight.FadeToAsync(0.2, 600, Easing.CubicOut);
		var modern = Board("FadeOut").PlayAsync(NewLight);
		await Task.WhenAll(legacy, modern);
		await Task.Delay(900);

		Step("dimmer");
		await SweepDimmerAsync();

		Step("result");
		ShowResult(LegacyTag, LegacyNumber, LegacyLight.Opacity);
		ShowResult(NewTag, NewNumber, NewLight.Opacity);
		Log("CHECK", string.Create(CultureInfo.InvariantCulture, $"dimmer={DimmerSlider.Value:F2} legacy={LegacyLight.Opacity:F4} new={NewLight.Opacity:F4}"));
		await Task.WhenAll(Board("SwapIn").PlayAsync(LegacyResult), Board("SwapIn").PlayAsync(NewResult));
	}

	// Moves the dimmer down to 45% and back up to 95% in a fixed time, the way a finger would.
	async Task SweepDimmerAsync()
	{
		const double down = 0.8, up = 0.8;
		var start = _clock.Elapsed;
		for (var t = 0.0; t < down + up; t = (_clock.Elapsed - start).TotalSeconds)
		{
			DimmerSlider.Value = t < down ? 1 - 0.55 * (t / down) : 0.45 + 0.5 * ((t - down) / up);
			await Task.Delay(16);
		}

		DimmerSlider.Value = 0.95;
	}

	// The verdict reads the light's actual opacity: it either follows the dimmer or it doesn't.
	void ShowResult(Label tag, Label number, double opacity)
	{
		var follows = Math.Abs(opacity - DimmerSlider.Value) < 0.005;
		tag.Text = follows ? "FOLLOWS" : "STUCK";
		number.Text = string.Create(CultureInfo.InvariantCulture, $"{opacity * 100:F0}%");
		number.TextColor = (Color)Resources[follows ? "Sun" : "Stamp"];
	}

	async Task CloseBackstageAsync()
	{
		if (!_backstageOpen)
			return;

		await Board("BackstageOut").PlayAsync(this);
		BackstageHost.InputTransparent = true;
		_backstageOpen = false;
	}

	void OnActTapped(object? sender, TappedEventArgs e)
	{
		if (e.Parameter is string text && int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
			Select(index);
	}

	void OnPlayClicked(object? sender, EventArgs e) => SetPlaying(_ledWall is null);

	void OnLikeTapped(object? sender, TappedEventArgs e) => _ = ToggleLikeAsync();

	void OnSkipClicked(object? sender, EventArgs e) => _ = SkipAsync();

	void OnMastheadTapped(object? sender, TappedEventArgs e) => _ = RunBackstageAsync();

	void OnBackstageTapped(object? sender, TappedEventArgs e) => _ = CloseBackstageAsync();

	Storyboard Board(string key) => (Storyboard)Resources[key];

	void Step(string name) => Log("STEP", name);

	void Log(string kind, string message)
		=> Console.WriteLine($"ANIM|{_clock.Elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}|{kind}|{message}");
}
