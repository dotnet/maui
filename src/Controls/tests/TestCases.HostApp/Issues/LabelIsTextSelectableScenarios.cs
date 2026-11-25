namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Label.IsTextSelectable scenarios keep layout identical", PlatformAffected.All)]
public class LabelIsTextSelectableScenarios : ContentPage
{
	const string Sample = "The quick brown fox jumps over the lazy dog and keeps running across the field";

	public static readonly string[] ScenarioIds =
	{
		"Plain", "Padding", "VAlignCenter", "VAlignEnd", "HAlignCenter", "HAlignEnd", "Html", "Formatted",
		"MaxLines", "HeadTrunc", "MiddleTrunc", "Rtl", "Styled", "Disabled", "TapLabel", "LongPressLabel",
	};

	readonly List<Label> _labels = new();
	readonly Label _status = new() { AutomationId = "Status", Text = "Selectable: True" };
	readonly Label _gestureLabel = new() { AutomationId = "GestureResult", Text = "Gesture: none" };
	bool _selectable = true;

	public LabelIsTextSelectableScenarios()
	{
		var toggle = new Button { AutomationId = "ToggleAll", Text = "Toggle", HeightRequest = 36 };
		toggle.Clicked += (s, e) =>
		{
			_selectable = !_selectable;
			foreach (var l in _labels)
				l.IsTextSelectable = _selectable;
			_status.Text = $"Selectable: {_selectable}";
		};

		var mutate = new Button { AutomationId = "Mutate", Text = "Mutate", HeightRequest = 36 };
		mutate.Clicked += (s, e) =>
		{
			foreach (var l in _labels.Where(l => l.AutomationId is "Plain" or "Padding" or "VAlignCenter" or "HAlignCenter" or "Styled"))
			{
				l.Text = "Changed at runtime: " + l.Text;
				l.FontSize += 2;
			}

			var padding = _labels.First(l => l.AutomationId == "Padding");
			padding.Padding = new Thickness(20, 12, 4, 12);

			var valign = _labels.First(l => l.AutomationId == "VAlignCenter");
			valign.VerticalTextAlignment = TextAlignment.End;
		};

		var hide = new Button { AutomationId = "HideToggle", Text = "Hide", HeightRequest = 36 };
		hide.Clicked += (s, e) =>
		{
			var l = _labels.First(l => l.AutomationId == "Plain");
			l.IsVisible = !l.IsVisible;
		};

		Label Add(string id, string text = Sample, Action<Label> configure = null)
		{
			var label = new Label
			{
				AutomationId = id,
				Text = text,
				FontSize = 12,
				BackgroundColor = Colors.LightYellow,
				IsTextSelectable = true,
			};
			configure?.Invoke(label);
			_labels.Add(label);
			return label;
		}

		var stack = new VerticalStackLayout { Spacing = 4, Padding = new Thickness(8, 0) };
		stack.Add(new HorizontalStackLayout { Spacing = 8, Children = { toggle, mutate, hide, _status } });
		stack.Add(new Entry { AutomationId = "ClearFocus", Placeholder = "Tap to clear the selection", HeightRequest = 34, FontSize = 12 });

		stack.Add(Add("Plain"));
		stack.Add(Add("Padding", configure: l => l.Padding = new Thickness(16, 8, 24, 8)));
		stack.Add(Add("VAlignCenter", "Center", l => { l.HeightRequest = 40; l.VerticalTextAlignment = TextAlignment.Center; }));
		stack.Add(Add("VAlignEnd", "End", l => { l.HeightRequest = 40; l.VerticalTextAlignment = TextAlignment.End; }));
		stack.Add(Add("HAlignCenter", Sample, l => l.HorizontalTextAlignment = TextAlignment.Center));
		stack.Add(Add("HAlignEnd", Sample, l => l.HorizontalTextAlignment = TextAlignment.End));
		stack.Add(Add("Html", "<b>Bold</b> and <i>italic</i> and <u>underlined</u> html text that wraps around", l => l.TextType = TextType.Html));

		var span = new Span { Text = "tap span", TextColor = Colors.Blue, TextDecorations = TextDecorations.Underline };
		span.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _gestureLabel.Text = "Gesture: span") });
		stack.Add(Add("Formatted", null, l => l.FormattedText = new FormattedString
		{
			Spans = { span, new Span { Text = " then more italic formatted text that wraps", FontAttributes = FontAttributes.Italic } }
		}));

		stack.Add(Add("Rtl", Sample, l => { l.FlowDirection = FlowDirection.RightToLeft; l.Padding = new Thickness(20, 0, 4, 0); }));
		stack.Add(Add("Styled", Sample, l =>
		{
			l.FontAttributes = FontAttributes.Bold;
			l.TextColor = Colors.DarkRed;
			l.LineHeight = 1.4;
			l.CharacterSpacing = 1;
			l.TextDecorations = TextDecorations.Underline;
			l.TextTransform = TextTransform.Uppercase;
		}));
		stack.Add(Add("Disabled", Sample, l => l.IsEnabled = false));
		stack.Add(Add("TapLabel", "Tap this label", l => l.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _gestureLabel.Text = "Gesture: label tap") })));
		stack.Add(Add("LongPressLabel", "Long press this label", l => l.GestureRecognizers.Add(new LongPressGestureRecognizer { Command = new Command(() => _gestureLabel.Text = "Gesture: long press") })));
		var shadowTap = new Label
		{
			AutomationId = "ShadowTapLabel",
			Text = "Shadow tap label",
			FontSize = 12,
			BackgroundColor = Colors.LightYellow,
			IsTextSelectable = true,
			Shadow = new Shadow { Radius = 2, Opacity = 0.3f, Offset = new Point(1, 1) },
		};
		shadowTap.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _gestureLabel.Text = "Gesture: shadow tap") });
		stack.Add(shadowTap);

		var lateTaps = 0;
		var lateTap = new Label { AutomationId = "LateTapLabel", Text = "Late selectable tap label", FontSize = 12, BackgroundColor = Colors.LightYellow };
		lateTap.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => _gestureLabel.Text = $"Gesture: late tap {++lateTaps}") });
		var enableLate = new Button { AutomationId = "EnableLate", Text = "Enable", HeightRequest = 36 };
		enableLate.Clicked += (s, e) => lateTap.IsTextSelectable = true;
		stack.Add(new HorizontalStackLayout { Spacing = 8, Children = { lateTap, enableLate } });

		// Last, because Android lays out truncated selectable text differently and would shift the labels below.
		stack.Add(Add("MaxLines", Sample + " " + Sample, l => { l.MaxLines = 2; l.LineBreakMode = LineBreakMode.TailTruncation; }));
		stack.Add(Add("HeadTrunc", Sample + " " + Sample, l => { l.MaxLines = 1; l.LineBreakMode = LineBreakMode.HeadTruncation; }));
		stack.Add(Add("MiddleTrunc", Sample + " " + Sample, l => { l.MaxLines = 1; l.LineBreakMode = LineBreakMode.MiddleTruncation; }));
		stack.Add(_gestureLabel);

		var collection = new CollectionView
		{
			AutomationId = "Collection",
			HeightRequest = 90,
			ItemsSource = Enumerable.Range(1, 30).Select(i => $"Item {i}: selectable label in a recycled cell").ToList(),
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { FontSize = 12, IsTextSelectable = true, Padding = 4 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
		};
		stack.Add(collection);

		Content = stack;
	}
}
