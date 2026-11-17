using System.Diagnostics;
using System.Globalization;
using Microsoft.Maui.Controls.Animations;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	readonly Stopwatch _stopwatch = Stopwatch.StartNew();
	bool _selected;
	bool _liked;
	bool _demoStarted;
	StoryboardPlayer? _typing;

	public MainPage()
	{
		InitializeComponent();

		// Initial states apply instantly: a group with no current state never animates.
		VisualStateManager.GoToState(Heart, "NotLiked");

		Trace("Card", Card, VisualElement.ScaleProperty);
		Trace("CardTitle", CardTitle, Label.TextColorProperty);
		Trace("Press", PressButton, VisualElement.ScaleProperty);
		Trace("Heart", Heart, VisualElement.ScaleProperty);
		Trace("Heart", Heart, Label.TextColorProperty);
		Trace("Shake", CodeBox, VisualElement.TranslationXProperty);
		Trace("Dot1", Dot1, VisualElement.TranslationYProperty);
		Trace("Legacy", LegacyBox, VisualElement.OpacityProperty);
		Trace("New", NewBox, VisualElement.OpacityProperty);

		Loaded += OnPageLoaded;
	}

	async void OnPageLoaded(object? sender, EventArgs e)
	{
		if (_demoStarted)
			return;

		_demoStarted = true;
		await Task.Delay(2500);
		await RunDemoAsync();
	}

	async Task RunDemoAsync()
	{
		await Step("VisualTransition: Normal → Selected (Spring 0.45)", () => SetCard(true), 1700);
		await Step("VisualTransition: Selected → Normal (CubicOut)", () => SetCard(false), 1400);

		await Step("Interrupted mid-flight: no jumps", () => SetCard(true), 160);
		SetCard(false);
		await Task.Delay(160);
		SetCard(true);
		await Task.Delay(1500);
		SetCard(false);
		await Task.Delay(1000);

		await Step("Built-in Pressed state, animated", () => VisualStateManager.GoToState(PressButton, "Pressed"), 250);
		VisualStateManager.GoToState(PressButton, "Normal");
		await Task.Delay(1200);

		await Step("Storyboard: keyframes + per-segment easing", () => _ = LikeAsync(), 1800);
		await Step("Storyboard: cues placed by the engine", () => _ = ShakeAsync(), 1400);
		await Step("Storyboard: Forever, three targets", StartTyping, 3700);
		await Step("Stop() releases the animated values", StopTyping, 1300);

		await Step("Fade both: FadeToAsync vs Storyboard", () => _ = FadeBothAsync(), 1600);
		await Step("Move the slider: which box still follows?", () => _ = SweepSliderAsync(), 3800);

		Caption.Text = "Done. Tap anything.";
		Log("STEP", "done");
	}

	async Task Step(string caption, Action action, int millisecondsAfter)
	{
		Caption.Text = caption;
		Log("STEP", caption);
		action();
		await Task.Delay(millisecondsAfter);
	}

	void OnCardTapped(object? sender, TappedEventArgs e) => SetCard(!_selected);

	void OnHeartTapped(object? sender, TappedEventArgs e) => _ = LikeAsync();

	void OnShakeTapped(object? sender, TappedEventArgs e) => _ = ShakeAsync();

	void OnTypingClicked(object? sender, EventArgs e)
	{
		if (_typing is null)
			StartTyping();
		else
			StopTyping();
	}

	void OnFadeClicked(object? sender, EventArgs e) => _ = FadeBothAsync();

	void SetCard(bool selected)
	{
		_selected = selected;
		var notifications = 0;
		void Count(object? s, System.ComponentModel.PropertyChangedEventArgs args)
		{
			if (args.PropertyName == nameof(Scale))
				notifications++;
		}

		Card.PropertyChanged += Count;
		VisualStateManager.GoToState(Card, selected ? "Selected" : "Normal");
		Card.PropertyChanged -= Count;

		// The state changes synchronously; the pinned properties raise nothing at the switch.
		Log("SWITCH", $"Card -> {(selected ? "Selected" : "Normal")}; Scale notifications during GoToState: {notifications}; Scale now {Card.Scale.ToString("F4", CultureInfo.InvariantCulture)}");
		Status.Text = $"Card is {(selected ? "Selected" : "Normal")}";
	}

	async Task LikeAsync()
	{
		_liked = !_liked;
		VisualStateManager.GoToState(Heart, _liked ? "Liked" : "NotLiked");
		if (!_liked)
			return;

		var result = await Board("LikePop").PlayAsync(this);
		Report("LikePop", result);
	}

	async Task ShakeAsync()
	{
		var result = await Board("Shake").PlayAsync(this);
		Report("Shake", result);
	}

	void StartTyping()
	{
		if (_typing is not null)
			return;

		_typing = Board("Typing").Begin(this);
		_typing.Completed += (_, args) => Report("Typing", args.Result);
		TypingButton.Text = "Stop typing";
		Status.Text = "Typing: Running (Forever)";
	}

	void StopTyping()
	{
		_typing?.Stop();
		_typing = null;
		TypingButton.Text = "Start typing";
		Log("CHECK", $"after Stop: Dot1.TranslationY={Dot1.TranslationY.ToString("F4", CultureInfo.InvariantCulture)}");
	}

	async Task FadeBothAsync()
	{
		var legacy = LegacyBox.FadeToAsync(0.2, 600, Easing.CubicOut);
		var modern = Board("FadeOut").PlayAsync(NewBox);
		await Task.WhenAll(legacy, modern);
		Report("FadeOut", modern.Result);
	}

	async Task SweepSliderAsync()
	{
		// Moves the slider like a user would. Both boxes bind Opacity to it.
		for (var value = OpacitySlider.Value; value > 0.5; value -= 0.02)
		{
			OpacitySlider.Value = value;
			await Task.Delay(20);
		}

		for (var value = 0.5; value <= 0.9; value += 0.02)
		{
			OpacitySlider.Value = value;
			await Task.Delay(20);
		}

		OpacitySlider.Value = 0.9;
		Status.Text = string.Create(CultureInfo.InvariantCulture, $"Slider 0.90 · FadeToAsync box {LegacyBox.Opacity:F2} · Storyboard box {NewBox.Opacity:F2}");
		Log("CHECK", string.Create(CultureInfo.InvariantCulture, $"slider=0.9 legacy={LegacyBox.Opacity:F4} new={NewBox.Opacity:F4}"));
	}

	Storyboard Board(string key) => (Storyboard)Resources[key];

	void Report(string name, AnimationResult result)
	{
		Status.Text = $"{name} → {result}";
		Log("RESULT", $"{name} -> {result}");
	}

	void Trace(string name, BindableObject element, BindableProperty property)
	{
		element.PropertyChanged += (_, args) =>
		{
			if (args.PropertyName == property.PropertyName)
				Log("TRACE", $"{name}|{property.PropertyName}|{Format(element.GetValue(property))}");
		};
	}

	void Log(string kind, string message)
		=> Console.WriteLine($"ANIM|{_stopwatch.Elapsed.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture)}|{kind}|{message}");

	static string Format(object? value) => value switch
	{
		double d => d.ToString("F4", CultureInfo.InvariantCulture),
		Color c => c.ToArgbHex(),
		_ => value?.ToString() ?? "null",
	};
}
