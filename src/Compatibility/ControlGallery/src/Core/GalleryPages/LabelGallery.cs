namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	public class LabelGallery : ContentPage
	{
		public LabelGallery()
		{
			var normal = new Label { Text = "Normal Label" };
			var center = new Label { Text = "Center Label" };
			var right = new Label { Text = "Right Label" };
			var moving = new Label { Text = "Move On Click" };
			var click = new Label { Text = "Click Label" };
			var rotate = new Label { Text = "Rotate Label" };
			var transparent = new Label { Text = "Transparent Label" };
			var color = new Label { Text = "Color Change" };
			var micro = new Label { Text = "Micro Label" };
			var small = new Label { Text = "Small Label" };
			var medium = new Label { Text = "Medium Label" };
			var large = new Label { Text = "Large Label", VerticalOptions = LayoutOptions.FillAndExpand, VerticalTextAlignment = TextAlignment.Center };
			var bold = new Label { Text = "Bold Label" };
			var italic = new Label { Text = "Italic Label" };
			var bolditalic = new Label { Text = "Bold Italic Label" };
			var customFont = new Label { Text = "Custom Font" };
			var italicfont = new Label { Text = "Custom Italic Font" };
			var boldfont = new Label { Text = "Custom Bold Font" };
			var bolditalicfont = new Label { Text = "Custom Bold Italic Font" };
			var toggleUnderline = new Label { Text = "Tap to toggle Underline", TextDecorations = TextDecorations.Underline };
			toggleUnderline.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { toggleUnderline.TextDecorations ^= TextDecorations.Underline; }) });
			var toggleStrike = new Label { Text = "Tap to toggle StrikeThrough", TextDecorations = TextDecorations.Strikethrough };
			toggleStrike.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { toggleStrike.TextDecorations ^= TextDecorations.Strikethrough; }) });
			var toggleBoth = new Label { Text = "Tap to toggle both", TextDecorations = TextDecorations.Strikethrough | TextDecorations.Underline };
			toggleBoth.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(() =>
				{
					toggleBoth.TextDecorations ^= TextDecorations.Strikethrough;
					toggleBoth.TextDecorations ^= TextDecorations.Underline;
				})
			});
			var huge = new Label
			{
				Text = "This is the label that never ends, yes it go on and on my friend. " +
				"Some people started catting it not knowing what it was, and they'll continue catting it forever just because...",
				LineBreakMode = LineBreakMode.WordWrap
			};
			var formatted = new Label
			{
				FormattedText = new FormattedString
				{
					Spans = {
						new Span {Text="FormattedStrings ", TextColor=Color.Blue, BackgroundColor = Color.Yellow, FontAttributes = FontAttributes.Bold, FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label))},
					}
				}
			};
			var underlineSpan = new Span { Text = "are ", TextColor = Color.Red, BackgroundColor = Color.Gray, TextDecorations = TextDecorations.Underline };
			var strikeSpan = new Span { Text = "not pretty!", TextColor = Color.Green, TextDecorations = TextDecorations.Strikethrough };
			formatted.FormattedText.Spans.Add(underlineSpan);
			formatted.FormattedText.Spans.Add(strikeSpan);

			var missingfont = new Label { Text = "Missing font: use default" };

			micro.FontSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));

			small.FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label));

			medium.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

			large.FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label));

			bold.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
			bold.FontAttributes = FontAttributes.Bold;

			italic.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
			italic.FontAttributes = FontAttributes.Italic;

			bolditalic.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
			bolditalic.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;

			string fontName;
			switch (Device.RuntimePlatform)
			{
				default:
				case Device.iOS:
					fontName = "Georgia";
					break;
				case Device.Android:
					fontName = "sans-serif-light";
					break;
				case Device.UWP:
					fontName = "Comic Sans MS";
					break;
			}

			customFont.FontFamily = fontName;
			customFont.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));

			italicfont.FontFamily = fontName;
			italicfont.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
			italicfont.FontAttributes = FontAttributes.Italic;

			boldfont.FontFamily = fontName;
			boldfont.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
			boldfont.FontAttributes = FontAttributes.Bold;

			bolditalicfont.FontFamily = fontName;
			bolditalicfont.FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label));
			bolditalicfont.FontAttributes = FontAttributes.Bold | FontAttributes.Italic;

			customFont.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(o =>
					{
						customFont.FontAttributes = FontAttributes.None;
						customFont.FontFamily = null;
						customFont.FontSize = 0;
					})
			});

			missingfont.FontFamily = "FooBar";
			missingfont.FontSize = 20;

			center.HorizontalTextAlignment = TextAlignment.Center;
			right.HorizontalTextAlignment = TextAlignment.End;
			int i = 1;
			click.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(o => click.Text = "Clicked " + i++) });
			rotate.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(o => rotate.RelRotateTo(180)) });
			transparent.Opacity = .5;
			moving.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(o => moving.HorizontalTextAlignment = TextAlignment.End) });

			color.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = new Command(o =>
				{
					color.TextColor = new Color(1, 0, 0);
					color.BackgroundColor = new Color(0, 1, 0);
				})
			});

			Thickness padding = new Thickness(20);
			// Padding Adjust for iPad
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(20, 20, 20, 60);

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Padding = padding,
					Children = {
						formatted,
						normal,
						center,
						right,
						huge,
						moving,
						click,
						rotate,
						transparent,
						color,
						micro,
						small,
						medium,
						large,
						bold,
						italic,
						bolditalic,
						toggleUnderline,
						toggleStrike,
						toggleBoth,
						customFont,
						italicfont,
						boldfont,
						bolditalicfont,
						missingfont,
					}
				}
			};
		}
	}
}
