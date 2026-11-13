namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34504, "[iOS] Span TapGestureRecognizer does not work on the second line of the span, if the span is wrapped to the next line", PlatformAffected.iOS)]
public class Issue34504 : NavigationPage
{
	public Issue34504() : base(new FirstPage()) { }

	// First page mirrors the sandbox MainPage — has the same span content so
	// the layout system is exercised before navigating to the second page.
	class FirstPage : ContentPage
	{
		public FirstPage()
		{
			void OnSpanTapped(object sender, TappedEventArgs e)
			{
				// no-op — first page just needs spans to exist
			}

			var label = BuildSpanLabel(OnSpanTapped);

			var navigateButton = new Button
			{
				Text = "Navigate to Test Page",
				AutomationId = "NavigateButton",
				HorizontalOptions = LayoutOptions.Fill,
			};
			navigateButton.Clicked += async (s, e) =>
				await Navigation.PushAsync(new SecondPage());

			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30, 0),
				Spacing = 25,
				Children =
				{
					new Label
					{
						Text = "Click the button below to navigate to the test page with wrapped span gestures.",
						FontSize = 14,
						TextColor = Colors.Gray,
					},
					navigateButton,
					new Border
					{
						StrokeThickness = 2,
						Stroke = Colors.Black,
						Padding = new Thickness(10),
						Content = label,
					},
				}
			};
		}
	}

	// Second page mirrors the sandbox TestPage — this is where the bug manifests on iOS 26+.
	public class SecondPage : ContentPage
	{
		public SecondPage()
		{
			var statusLabel = new Label
			{
				AutomationId = "StatusLabel",
				Text = "Tap status will appear here",
				FontSize = 14,
				TextColor = Colors.Black,
			};

			void OnSpanTapped(object sender, TappedEventArgs e)
			{
				statusLabel.Text = "Success";
			}

			var spanLabel = BuildSpanLabel(OnSpanTapped);
			spanLabel.AutomationId = "SpanLabel";

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					Padding = new Thickness(30, 0),
					Spacing = 25,
					Children =
					{
						new Label
						{
							Text = "iOS TapGesture Issue Demonstration",
							FontSize = 18,
							FontAttributes = FontAttributes.Bold,
						},
						new Label
						{
							Text = "Tap on the colored text below. On iOS, gestures on wrapped lines may not work.",
							FontSize = 12,
							TextColor = Colors.Gray,
						},
						new Border
						{
							StrokeThickness = 2,
							Stroke = Colors.Black,
							Padding = new Thickness(10),
							Content = spanLabel,
						},
						statusLabel,
					}
				}
			};
		}
	}

	static Label BuildSpanLabel(EventHandler<TappedEventArgs> onTapped)
	{
		Span MakeSpan(string text, Color color)
		{
			var span = new Span
			{
				Text = text,
				TextColor = color,
				TextDecorations = TextDecorations.Underline,
			};
			var tap = new TapGestureRecognizer();
			tap.Tapped += onTapped;
			span.GestureRecognizers.Add(tap);
			return span;
		}

		var fs = new FormattedString();
		fs.Spans.Add(MakeSpan("Hello,This is a test. Hello,This is a test. Hello,This is a test.", Colors.Blue));
		fs.Spans.Add(MakeSpan("Hello,This is a test1. Hello,This is a test1. Hello,This is a test1.", Colors.Red));
		fs.Spans.Add(MakeSpan("Hello,This is a test2. Hello,This is a test2. Hello,This is a test2.", Colors.Green));
		fs.Spans.Add(MakeSpan("Hello,This is a test4. Hello,This is a test4. Hello,This is a test4.", Colors.Orange));
		fs.Spans.Add(MakeSpan("Hello,This is a test3. Hello,This is a test3. Hello,This is a test3.", Colors.Purple));
		fs.Spans.Add(new Span { Text = " World!", FontAttributes = FontAttributes.Bold });

		return new Label
		{
			FormattedText = fs,
			BackgroundColor = Colors.Transparent,
			LineBreakMode = LineBreakMode.WordWrap,
			MaximumWidthRequest = 300,
		};
	}
}
