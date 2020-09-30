using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls
{
	internal class LabelCoreGalleryPage : CoreGalleryPage<Label>
	{
		protected override bool SupportsFocus
		{
			get { return false; }
		}

		protected override void InitializeElement(Label element)
		{
			element.Text = "I am a label's text.";
		}

		protected override void Build(StackLayout stackLayout)
		{
			base.Build(stackLayout);

#pragma warning disable 618
			var namedSizeMediumBoldContainer = new ViewContainer<Label>(Test.Label.FontAttibutesBold, new Label { Text = "Medium Bold Font", Font = Font.SystemFontOfSize(NamedSize.Medium, FontAttributes.Bold) });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeMediumItalicContainer = new ViewContainer<Label>(Test.Label.FontAttributesItalic, new Label { Text = "Medium Italic Font", Font = Font.SystemFontOfSize(NamedSize.Medium, FontAttributes.Italic) });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeMediumUnderlineContainer = new ViewContainer<Label>(Test.Label.TextDecorationUnderline, new Label { Text = "Medium Underlined Font", Font = Font.SystemFontOfSize(NamedSize.Medium), TextDecorations = TextDecorations.Underline });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeMediumStrikeContainer = new ViewContainer<Label>(Test.Label.TextDecorationStrike, new Label { Text = "Medium StrikeThrough Font", Font = Font.SystemFontOfSize(NamedSize.Medium), TextDecorations = TextDecorations.Strikethrough });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeLargeContainer = new ViewContainer<Label>(Test.Label.FontNamedSizeLarge, new Label { Text = "Large Font", Font = Font.SystemFontOfSize(NamedSize.Large) });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeMediumContainer = new ViewContainer<Label>(Test.Label.FontNamedSizeMedium, new Label { Text = "Medium Font", Font = Font.SystemFontOfSize(NamedSize.Medium) });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeMicroContainer = new ViewContainer<Label>(Test.Label.FontNamedSizeMicro, new Label { Text = "Micro Font", Font = Font.SystemFontOfSize(NamedSize.Micro) });
#pragma warning restore 618

#pragma warning disable 618
			var namedSizeSmallContainer = new ViewContainer<Label>(Test.Label.FontNamedSizeSmall, new Label { Text = "Small Font", Font = Font.SystemFontOfSize(NamedSize.Small) });
#pragma warning restore 618

			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { BackgroundColor = Color.Red, TextColor = Color.Olive, Text = "Span 1 " });

			Span span = new Span { BackgroundColor = Color.Black, TextColor = Color.White, Text = "Span 2 (tap me) " };
			span.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Congratulations!", "This is a tapped span", "ok")) });
			formattedString.Spans.Add(span);

			formattedString.Spans.Add(new Span { BackgroundColor = Color.Pink, TextColor = Color.Purple, Text = "Span 3" });

			var formattedTextContainer = new ViewContainer<Label>(Test.Label.FormattedText, new Label { FormattedText = formattedString });

			const string longText = "Lorem ipsum dolor sit amet, cu mei malis petentium, dolor tempor delicata no qui, eos ex vitae utinam vituperata. Utroque habemus philosophia ut mei, doctus placerat eam cu. An inermis scaevola pro, quo legimus deleniti ei, equidem docendi urbanitas ea eum. Saepe doctus ut pri. Nec ex wisi dolorem. Duo dolor vituperatoribus ea. Id purto instructior per. Nec partem accusamus ne. Qui ad saepe accumsan appellantur, duis omnesque has et, vim nihil nemore scaevola ne. Ei populo appetere recteque xum, meliore splendide appellantur vix id.";
			var lineBreakModeCharacterWrapContainer = new ViewContainer<Label>(Test.Label.LineBreakModeCharacterWrap, new Label { Text = longText, LineBreakMode = LineBreakMode.CharacterWrap });
			var lineBreakModeHeadTruncationContainer = new ViewContainer<Label>(Test.Label.LineBreakModeHeadTruncation, new Label { Text = longText, LineBreakMode = LineBreakMode.HeadTruncation });
			var lineBreakModeMiddleTruncationContainer = new ViewContainer<Label>(Test.Label.LineBreakModeMiddleTruncation, new Label { Text = longText, LineBreakMode = LineBreakMode.MiddleTruncation });
			var lineBreakModeNoWrapContainer = new ViewContainer<Label>(Test.Label.LineBreakModeNoWrap, new Label { Text = longText, LineBreakMode = LineBreakMode.NoWrap });
			var lineBreakModeTailTruncationContainer = new ViewContainer<Label>(Test.Label.LineBreakModeTailTruncation, new Label { Text = longText, LineBreakMode = LineBreakMode.TailTruncation });
			var lineBreakModeWordWrapContainer = new ViewContainer<Label>(Test.Label.LineBreakModeWordWrap, new Label { Text = longText, LineBreakMode = LineBreakMode.WordWrap });

			var textContainer = new ViewContainer<Label>(Test.Label.Text, new Label { Text = "I should have text" });
			var textColorContainer = new ViewContainer<Label>(Test.Label.TextColor, new Label { Text = "I should have lime text", TextColor = Color.Lime });

			const int alignmentTestsHeightRequest = 100;
			const int alignmentTestsWidthRequest = 100;

			var xAlignCenterContainer = new ViewContainer<Label>(Test.Label.HorizontalTextAlignmentCenter,
				new Label
				{
					Text = "HorizontalTextAlignment Center",
					HorizontalTextAlignment = TextAlignment.Center,
					HeightRequest = alignmentTestsHeightRequest,
					WidthRequest = alignmentTestsWidthRequest
				}
			);

			var xAlignEndContainer = new ViewContainer<Label>(Test.Label.HorizontalTextAlignmentEnd,
				new Label
				{
					Text = "HorizontalTextAlignment End",
					HorizontalTextAlignment = TextAlignment.End,
					HeightRequest = alignmentTestsHeightRequest,
					WidthRequest = alignmentTestsWidthRequest
				}
			);

			var xAlignStartContainer = new ViewContainer<Label>(Test.Label.HorizontalTextAlignmentStart,
				new Label
				{
					Text = "HorizontalTextAlignment Start",
					HorizontalTextAlignment = TextAlignment.Start,
					HeightRequest = alignmentTestsHeightRequest,
					WidthRequest = alignmentTestsWidthRequest
				}
			);

			var yAlignCenterContainer = new ViewContainer<Label>(Test.Label.VerticalTextAlignmentCenter,
				new Label
				{
					Text = "VerticalTextAlignment Start",
					VerticalTextAlignment = TextAlignment.Center,
					HeightRequest = alignmentTestsHeightRequest,
					WidthRequest = alignmentTestsWidthRequest,
					BackgroundColor = Color.Pink
				}
			);

			var yAlignEndContainer = new ViewContainer<Label>(Test.Label.VerticalTextAlignmentEnd,
				new Label
				{
					Text = "VerticalTextAlignment End",
					VerticalTextAlignment = TextAlignment.End,
					HeightRequest = alignmentTestsHeightRequest,
					WidthRequest = alignmentTestsWidthRequest,
					BackgroundColor = Color.Pink
				}
			);

			var yAlignStartContainer = new ViewContainer<Label>(Test.Label.VerticalTextAlignmentStart,
				new Label
				{
					Text = "VerticalTextAlignment Start",
					VerticalTextAlignment = TextAlignment.Start,
					HeightRequest = alignmentTestsHeightRequest,
					WidthRequest = alignmentTestsWidthRequest,
					BackgroundColor = Color.Pink
				}
			);

			var styleTitleContainer = new ViewContainer<Label>(Test.Device.Styles,
				new Label
				{
					Text = "Device.Styles.TitleStyle",
					Style = Device.Styles.TitleStyle
				}
			);

			var styleSubtitleContainer = new ViewContainer<Label>(Test.Device.Styles,
				new Label
				{
					Text = "Device.Styles.SubtitleStyle",
					Style = Device.Styles.SubtitleStyle
				}
			);

			var styleBodyContainer = new ViewContainer<Label>(Test.Device.Styles,
				new Label
				{
					Text = "Device.Styles.BodyStyle",
					Style = Device.Styles.BodyStyle
				}
			);

			var styleCaptionContainer = new ViewContainer<Label>(Test.Device.Styles,
				new Label
				{
					Text = "Device.Styles.CaptionStyle",
					Style = Device.Styles.CaptionStyle,
				}
			);

			var maxlinesContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2
				}
			);

			var maxlinesCharWrapContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2,
					LineBreakMode = LineBreakMode.CharacterWrap
				}
			);

			var maxlinesHeadTruncContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2,
					LineBreakMode = LineBreakMode.HeadTruncation
				}
			);

			var maxlinesMiddleTruncContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2,
					LineBreakMode = LineBreakMode.MiddleTruncation
				}
			);

			var maxlinesNoWrapContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2,
					LineBreakMode = LineBreakMode.NoWrap
				}
			);

			var maxlinesTailTruncContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2,
					LineBreakMode = LineBreakMode.TailTruncation
				}
			);

			var maxlinesWordWrapContainer = new ViewContainer<Label>(Test.Label.MaxLines,
				new Label
				{
					Text = longText,
					MaxLines = 2,
					LineBreakMode = LineBreakMode.WordWrap
				}
			);

			var formattedString2 = new FormattedString();
			formattedString2.Spans.Add(new Span { BackgroundColor = Color.Red, TextColor = Color.Olive, Text = "Span 1 " });
			var span2 = new Span { BackgroundColor = Color.Black, TextColor = Color.White, Text = "Span 2 (tap me) " };
			span2.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Congratulations!", "This is a tapped span", "ok")) });
			formattedString2.Spans.Add(span2);
			formattedString2.Spans.Add(new Span { BackgroundColor = Color.Pink, TextColor = Color.Purple, Text = "Span 3" });

			var paddingContainer = new ViewContainer<Label>(Test.Label.Padding,
				new Label
				{
					FormattedText = formattedString2,
					BackgroundColor = Color.Yellow,
					Padding = new Thickness(40, 20)
				}
			);

			var htmlLabelContainer = new ViewContainer<Label>(Test.Label.HtmlTextType,
				new Label
				{
					Text = "<h1>Hello world!</h1>",
					TextType = TextType.Html
				});

			var htmlLabelMultipleLinesContainer = new ViewContainer<Label>(Test.Label.HtmlTextTypeMultipleLines,
				new Label
				{
					Text = "<h1>Hello world!</h1><p>Lorem <strong>ipsum</strong> bla di bla <i>blabla</i> blablabl&nbsp;ablabla & blablablablabl ablabl ablablabl ablablabla blablablablablablab lablablabla blablab lablablabla blablabl ablablablab lablabla blab lablablabla blablab lablabla blablablablab lablabla blablab lablablabl ablablabla blablablablablabla blablabla</p>",
					TextType = TextType.Html,
					MaxLines = 3
				});

			var toggleLabel = new Label
			{
				TextType = TextType.Html,
				Text = "<h1 style=\"color: red;\">Hello world!</h1><p>Lorem <strong>ipsum</strong></p>"

			};

			var htmlLabelProperties = new ViewContainer<Label>(Test.Label.HtmlTextLabelProperties, new Label
			{
				Text = "<h1>End aligned. Green. ä</h1>",
				TextType = TextType.Html,
				HorizontalTextAlignment = TextAlignment.Center,
				TextColor = Color.Green,
			});

			var gestureRecognizer = new TapGestureRecognizer();

			gestureRecognizer.Tapped += (s, a) =>
			{
				toggleLabel.TextType = toggleLabel.TextType == TextType.Html ? TextType.Text : TextType.Html;
			};

			toggleLabel.GestureRecognizers.Add(gestureRecognizer);

			var toggleHtmlPlainTextLabelContainer = new ViewContainer<Label>(Test.Label.TextTypeToggle,
				toggleLabel);

			Add(namedSizeMediumBoldContainer);
			Add(namedSizeMediumItalicContainer);
			Add(namedSizeMediumUnderlineContainer);
			Add(namedSizeMediumStrikeContainer);
			Add(namedSizeLargeContainer);
			Add(namedSizeMediumContainer);
			Add(namedSizeMicroContainer);
			Add(namedSizeSmallContainer);
			Add(formattedTextContainer);
			Add(lineBreakModeCharacterWrapContainer);
			Add(lineBreakModeHeadTruncationContainer);
			Add(lineBreakModeMiddleTruncationContainer);
			Add(lineBreakModeNoWrapContainer);
			Add(lineBreakModeTailTruncationContainer);
			Add(lineBreakModeWordWrapContainer);
			Add(textContainer);
			Add(textColorContainer);
			Add(xAlignCenterContainer);
			Add(xAlignEndContainer);
			Add(xAlignStartContainer);
			Add(yAlignCenterContainer);
			Add(yAlignEndContainer);
			Add(yAlignStartContainer);
			Add(styleTitleContainer);
			Add(styleSubtitleContainer);
			Add(styleBodyContainer);
			Add(styleCaptionContainer);
			Add(maxlinesContainer);
			Add(maxlinesCharWrapContainer);
			Add(maxlinesHeadTruncContainer);
			Add(maxlinesMiddleTruncContainer);
			Add(maxlinesNoWrapContainer);
			Add(maxlinesTailTruncContainer);
			Add(maxlinesWordWrapContainer);
			Add(paddingContainer);
			Add(htmlLabelContainer);
			Add(htmlLabelMultipleLinesContainer);
			Add(htmlLabelProperties);
			Add(toggleHtmlPlainTextLabelContainer);
		}
	}
}