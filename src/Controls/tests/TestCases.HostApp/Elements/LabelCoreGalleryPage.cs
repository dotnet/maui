namespace Maui.Controls.Sample;

internal class LabelCoreGalleryPage : CoreGalleryPage<Label>
{
	protected override bool SupportsFocus => false;

	protected override void InitializeElement(Label element)
	{
		element.Text = "I am a label's text.";
	}

	protected override void Build()
	{
		base.Build();

		// demonstrates that formatted text appears correctly
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { BackgroundColor = Colors.Red, TextColor = Colors.Olive, Text = "Span 1 " });
			var span = new Span { BackgroundColor = Colors.Black, TextColor = Colors.White, Text = "Span 2 (tap me) " };
			span.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlertAsync("Congratulations!", "This is a tapped span", "ok")) });
			formattedString.Spans.Add(span);
			formattedString.Spans.Add(new Span { BackgroundColor = Colors.Pink, TextColor = Colors.Purple, Text = "Span 3" });
			var formattedTextContainer = new ViewContainer<Label>(Test.Label.FormattedText, new Label { FormattedText = formattedString });
			Add(formattedTextContainer);
		}

		// demonstrates that tapping on a span is correctly handled
		{
			EventViewContainer<Label> spanTappedContainer = null!;
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { BackgroundColor = Colors.Red, TextColor = Colors.Olive, Text = "Span 1 " });
			var span = new Span { BackgroundColor = Colors.Black, TextColor = Colors.White, Text = "Span 2 (tap me)" };
			span.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => spanTappedContainer.EventFired()) });
			formattedString.Spans.Add(span);
			formattedString.Spans.Add(new Span { BackgroundColor = Colors.Pink, TextColor = Colors.Purple, Text = " Span 3" });
			spanTappedContainer = new EventViewContainer<Label>(Test.FormattedString.SpanTapped, new Label { FormattedText = formattedString });
			Add(spanTappedContainer);
		}

		const string longText = "Lorem ipsum dolor sit amet, cu mei malis petentium, dolor tempor delicata no qui, eos ex vitae utinam vituperata. Utroque habemus philosophia ut mei, doctus placerat eam cu. An inermis scaevola pro, quo legimus deleniti ei, equidem docendi urbanitas ea eum. Saepe doctus ut pri. Nec ex wisi dolorem. Duo dolor vituperatoribus ea. Id purto instructior per. Nec partem accusamus ne. Qui ad saepe accumsan appellantur, duis omnesque has et, vim nihil nemore scaevola ne. Ei populo appetere recteque xum, meliore splendide appellantur vix id.";

		{
			var label = new Label { Text = longText, LineBreakMode = LineBreakMode.CharacterWrap };
			var lineBreakModeCharacterWrapContainer = new ViewContainer<Label>(Test.Label.LineBreakModeCharacterWrap, label);
			Add(lineBreakModeCharacterWrapContainer);
		}

		{
			var label = new Label { Text = longText, LineBreakMode = LineBreakMode.HeadTruncation };
			var lineBreakModeHeadTruncationContainer = new ViewContainer<Label>(Test.Label.LineBreakModeHeadTruncation, label);
			Add(lineBreakModeHeadTruncationContainer);
		}

		{
			var label = new Label { Text = longText, LineBreakMode = LineBreakMode.MiddleTruncation };
			var lineBreakModeMiddleTruncationContainer = new ViewContainer<Label>(Test.Label.LineBreakModeMiddleTruncation, label);
			Add(lineBreakModeMiddleTruncationContainer);
		}

		{
			var label = new Label { Text = longText, LineBreakMode = LineBreakMode.NoWrap };
			var lineBreakModeNoWrapContainer = new ViewContainer<Label>(Test.Label.LineBreakModeNoWrap, label);
			Add(lineBreakModeNoWrapContainer);
		}

		{
			var label = new Label { Text = longText, LineBreakMode = LineBreakMode.TailTruncation };
			var lineBreakModeTailTruncationContainer = new ViewContainer<Label>(Test.Label.LineBreakModeTailTruncation, label);
			Add(lineBreakModeTailTruncationContainer);
		}

		{
			var label = new Label { Text = longText, LineBreakMode = LineBreakMode.WordWrap };
			var lineBreakModeWordWrapContainer = new ViewContainer<Label>(Test.Label.LineBreakModeWordWrap, label);
			Add(lineBreakModeWordWrapContainer);
		}

		{
			var label = new Label { Text = "I should have text" };
			var textContainer = new ViewContainer<Label>(Test.Label.Text, label);
			Add(textContainer);
		}

		{
			var label = new Label { Text = "I should have lime text", TextColor = Colors.Lime };
			var textColorContainer = new ViewContainer<Label>(Test.Label.TextColor, label);
			Add(textColorContainer);
		}

		const int alignmentTestsHeightRequest = 100;
		const int alignmentTestsWidthRequest = 100;

		{
			var label = new Label
			{
				Text = "HorizontalTextAlignment Center",
				HorizontalTextAlignment = TextAlignment.Center,
				HeightRequest = alignmentTestsHeightRequest,
				WidthRequest = alignmentTestsWidthRequest
			};
			var xAlignCenterContainer = new ViewContainer<Label>(Test.Label.HorizontalTextAlignmentCenter, label);
			Add(xAlignCenterContainer);
		}

		{
			var label = new Label
			{
				Text = "HorizontalTextAlignment End",
				HorizontalTextAlignment = TextAlignment.End,
				HeightRequest = alignmentTestsHeightRequest,
				WidthRequest = alignmentTestsWidthRequest
			};
			var xAlignEndContainer = new ViewContainer<Label>(Test.Label.HorizontalTextAlignmentEnd, label);
			Add(xAlignEndContainer);
		}

		{
			var label = new Label
			{
				Text = "HorizontalTextAlignment Start",
				HorizontalTextAlignment = TextAlignment.Start,
				HeightRequest = alignmentTestsHeightRequest,
				WidthRequest = alignmentTestsWidthRequest
			};
			var xAlignStartContainer = new ViewContainer<Label>(Test.Label.HorizontalTextAlignmentStart, label);
			Add(xAlignStartContainer);
		}

		{
			var label = new Label
			{
				Text = "VerticalTextAlignment Start",
				VerticalTextAlignment = TextAlignment.Center,
				HeightRequest = alignmentTestsHeightRequest,
				WidthRequest = alignmentTestsWidthRequest,
				BackgroundColor = Colors.Pink
			};
			var yAlignCenterContainer = new ViewContainer<Label>(Test.Label.VerticalTextAlignmentCenter, label);
			Add(yAlignCenterContainer);
		}

		{
			var label = new Label
			{
				Text = "VerticalTextAlignment End",
				VerticalTextAlignment = TextAlignment.End,
				HeightRequest = alignmentTestsHeightRequest,
				WidthRequest = alignmentTestsWidthRequest,
				BackgroundColor = Colors.Pink
			};
			var yAlignEndContainer = new ViewContainer<Label>(Test.Label.VerticalTextAlignmentEnd, label);
			Add(yAlignEndContainer);
		}

		{
			var label = new Label
			{
				Text = "VerticalTextAlignment Start",
				VerticalTextAlignment = TextAlignment.Start,
				HeightRequest = alignmentTestsHeightRequest,
				WidthRequest = alignmentTestsWidthRequest,
				BackgroundColor = Colors.Pink
			};
			var yAlignStartContainer = new ViewContainer<Label>(Test.Label.VerticalTextAlignmentStart, label);
			Add(yAlignStartContainer);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2
			};
			var maxlinesContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
			Add(maxlinesContainer);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.CharacterWrap
			};
			var maxlinesCharWrapContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
			Add(maxlinesCharWrapContainer);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.HeadTruncation
			};
			var maxlinesHeadTruncContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
			Add(maxlinesHeadTruncContainer);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.MiddleTruncation
			};
			var maxlinesMiddleTruncContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
			Add(maxlinesMiddleTruncContainer);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.NoWrap
			};
			var maxlinesNoWrapContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
			Add(maxlinesNoWrapContainer);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.TailTruncation
			};
			var maxlinesTailTruncContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
		}

		{
			var label = new Label
			{
				Text = longText,
				MaxLines = 2,
				LineBreakMode = LineBreakMode.WordWrap
			};
			var maxlinesWordWrapContainer = new ViewContainer<Label>(Test.Label.MaxLines, label);
			Add(maxlinesWordWrapContainer);
		}

		{
			var formattedString2 = new FormattedString();
			formattedString2.Spans.Add(new Span { BackgroundColor = Colors.Red, TextColor = Colors.Olive, Text = "Span 1 " });
			var span2 = new Span { BackgroundColor = Colors.Black, TextColor = Colors.White, Text = "Span 2 (tap me) " };
			span2.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlertAsync("Congratulations!", "This is a tapped span", "ok")) });
			formattedString2.Spans.Add(span2);
			formattedString2.Spans.Add(new Span { BackgroundColor = Colors.Pink, TextColor = Colors.Purple, Text = "Span 3" });
			var label = new Label
			{
				FormattedText = formattedString2,
				BackgroundColor = Colors.Yellow,
				Padding = new Thickness(40, 20)
			};
			var paddingContainer = new ViewContainer<Label>(Test.Label.Padding, label);
			Add(paddingContainer);
		}

		{
			var label = new Label
			{
				Text = "<h1>Hello world!</h1>",
				TextType = TextType.Html
			};
			var htmlLabelContainer = new ViewContainer<Label>(Test.Label.HtmlTextType, label);
			Add(htmlLabelContainer);
		}

		{
			var label = new Label
			{
				Text = "<h1>Broken Html!<h1>",
				TextType = TextType.Html
			};
			var htmlLabelContainer = new ViewContainer<Label>(Test.Label.BrokenHtmlTextType, label);
			Add(htmlLabelContainer);
		}

		{
			var label = new Label
			{
				Text = "<h1>Hello world!</h1><p>Lorem <strong>ipsum</strong> bla di bla <i>blabla</i> blablabl&nbsp;ablabla & blablablablabl ablabl ablablabl ablablabla blablablablablablab lablablabla blablab lablablabla blablabl ablablablab lablabla blab lablablabla blablab lablabla blablablablab lablabla blablab lablablabl ablablabla blablablablablabla blablabla</p>",
				TextType = TextType.Html,
				MaxLines = 3
			};
			var htmlLabelMultipleLinesContainer = new ViewContainer<Label>(Test.Label.HtmlTextTypeMultipleLines, label);
			Add(htmlLabelMultipleLinesContainer);
		}

		{
			var label = new Label
			{
				Text = "<h1>End aligned. Green. ä</h1>",
				TextType = TextType.Html,
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Colors.Green,
			};
			var htmlLabelProperties = new ViewContainer<Label>(Test.Label.HtmlTextLabelProperties, label);
			Add(htmlLabelProperties);
		}

		{
			var toggleLabel = new Label
			{
				TextType = TextType.Html,
				Text = "<h1 style=\"color: red;\">Hello world!</h1><p>Lorem <strong>ipsum</strong></p>"

			};
			var gestureRecognizer = new TapGestureRecognizer();
			gestureRecognizer.Tapped += (s, a) =>
			{
				toggleLabel.TextType = toggleLabel.TextType == TextType.Html ? TextType.Text : TextType.Html;
			};
			toggleLabel.GestureRecognizers.Add(gestureRecognizer);
			var toggleHtmlPlainTextLabelContainer = new ViewContainer<Label>(Test.Label.TextTypeToggle, toggleLabel);
			Add(toggleHtmlPlainTextLabelContainer);
		}

		{
			var label = new Label
			{
				FontFamily = "FA",
				FontSize = 48,
				Text = "\xf133",
				WidthRequest = 48,
				HeightRequest = 48,
			};
			var familyContainer = new StateViewContainer<Label>(Test.Label.FontFamily, label);
			familyContainer.StateChangeButton.Clicked += (s, a) =>
			{
				label.FontFamily = label.FontFamily == "FA" ? "Ion" : "FA";
				label.Text = label.FontFamily == "FA" ? "\xf133" : "\xf30c";
			};
			Add(familyContainer);
		}
	}
}
