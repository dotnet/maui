using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class LabelGallery : ContentPage
	{
		public LabelGallery ()
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
			var large = new Label { Text = "Large Label", VerticalOptions = LayoutOptions.FillAndExpand, VerticalTextAlignment = TextAlignment.Center};
			var bold = new Label { Text = "Bold Label" };
			var italic = new Label { Text = "Italic Label" };
			var bolditalic = new Label { Text = "Bold Italic Label" };
			var customFont = new Label { Text = "Custom Font" };
			var italicfont = new Label { Text = "Custom Italic Font" };
			var boldfont = new Label { Text = "Custom Bold Font" };
			var bolditalicfont = new Label { Text = "Custom Bold Italic Font" };
			var toggleUnderline = new Label { Text = "Tap to toggle Underline", TextDecorations = TextDecorations.Underline };
			toggleUnderline.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(()=> { toggleUnderline.TextDecorations ^= TextDecorations.Underline; }) });
			var toggleStrike = new Label { Text = "Tap to toggle StrikeThrough", TextDecorations = TextDecorations.Strikethrough };
			toggleStrike.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { toggleStrike.TextDecorations ^= TextDecorations.Strikethrough; }) });
			var toggleBoth = new Label { Text = "Tap to toggle both", TextDecorations = TextDecorations.Strikethrough | TextDecorations.Underline };
			toggleBoth.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => { toggleBoth.TextDecorations ^= TextDecorations.Strikethrough;
																										toggleBoth.TextDecorations ^= TextDecorations.Underline;
																								}) });
			var huge = new Label {
				Text = "This is the label that never ends, yes it go on and on my friend. " +
				"Some people started catting it not knowing what it was, and they'll continue catting it forever just because...",
				LineBreakMode = LineBreakMode.WordWrap
			};
			var formatted = new Label { FormattedText = new FormattedString { 
					Spans = { 
#pragma warning disable 618
						new Span {Text="FormattedStrings ", TextColor=Color.Blue, BackgroundColor = Color.Yellow, Font = Font.BoldSystemFontOfSize (NamedSize.Large)},
#pragma warning restore 618
					}
				} };
			var underlineSpan = new Span { Text = "are ", TextColor = Color.Red, BackgroundColor = Color.Gray, TextDecorations = TextDecorations.Underline };
			var strikeSpan = new Span { Text = "not pretty!", TextColor = Color.Green, TextDecorations = TextDecorations.Strikethrough };
			formatted.FormattedText.Spans.Add(underlineSpan);
			formatted.FormattedText.Spans.Add(strikeSpan);

			var missingfont = new Label { Text = "Missing font: use default" };

#pragma warning disable 618
			micro.Font = Font.SystemFontOfSize (NamedSize.Micro);
#pragma warning restore 618

#pragma warning disable 618
			small.Font = Font.SystemFontOfSize (NamedSize.Small);
#pragma warning restore 618

#pragma warning disable 618
			medium.Font = Font.SystemFontOfSize (NamedSize.Medium);
#pragma warning restore 618

#pragma warning disable 618
			large.Font = Font.SystemFontOfSize (NamedSize.Large);
#pragma warning restore 618

#pragma warning disable 618
			bold.Font = Font.SystemFontOfSize (NamedSize.Medium, FontAttributes.Bold);
#pragma warning restore 618

#pragma warning disable 618
			italic.Font = Font.SystemFontOfSize (NamedSize.Medium, FontAttributes.Italic);
#pragma warning restore 618

#pragma warning disable 618
			bolditalic.Font = Font.SystemFontOfSize (NamedSize.Medium, FontAttributes.Bold | FontAttributes.Italic);
#pragma warning restore 618
			string fontName;
			switch (Device.RuntimePlatform) {
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
			var font = Font.OfSize (fontName, NamedSize.Medium);
#pragma warning disable 618
			customFont.Font = font;
#pragma warning restore 618

#pragma warning disable 618
			italicfont.Font = font.WithAttributes (FontAttributes.Italic);
#pragma warning restore 618

#pragma warning disable 618
			boldfont.Font = font.WithAttributes (FontAttributes.Bold);
#pragma warning restore 618

#pragma warning disable 618
			bolditalicfont.Font = font.WithAttributes (FontAttributes.Bold | FontAttributes.Italic);
#pragma warning restore 618

#pragma warning disable 618
			customFont.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o => customFont.Font = Font.Default)});
#pragma warning restore 618

#pragma warning disable 618
			missingfont.Font = Font.OfSize ("FooBar", 20);
#pragma warning restore 618
			center.HorizontalTextAlignment = TextAlignment.Center;
			right.HorizontalTextAlignment = TextAlignment.End;
			int i = 1;
			click.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o=>click.Text = "Clicked " + i++)});
			rotate.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o=>rotate.RelRotateTo (180))});
			transparent.Opacity = .5;
			moving.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o=>moving.HorizontalTextAlignment = TextAlignment.End)});

			color.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o=>{
				color.TextColor = new Color (1, 0, 0);
				color.BackgroundColor = new Color (0, 1, 0);
			})});

			Thickness padding = new Thickness (20);
			// Padding Adjust for iPad
			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(20, 20, 20, 60);

			Content = new ScrollView {
				Content = new StackLayout {
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
