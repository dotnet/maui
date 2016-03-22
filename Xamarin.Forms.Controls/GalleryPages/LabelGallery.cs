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
			var huge = new Label {
				Text = "This is the label that never ends, yes it go on and on my friend. " +
				"Some people started catting it not knowing what it was, and they'll continue catting it forever just because...",
				LineBreakMode = LineBreakMode.WordWrap
			};
			var formatted = new Label { FormattedText = new FormattedString { 
					Spans = { 
						new Span {Text="FormattedStrings ", ForegroundColor=Color.Blue, BackgroundColor = Color.Yellow, Font = Font.BoldSystemFontOfSize (NamedSize.Large)},
						new Span {Text="are ", ForegroundColor=Color.Red, BackgroundColor = Color.Gray},
						new Span {Text="not pretty!", ForegroundColor = Color.Green,},
					}
				} };
			var missingfont = new Label { Text = "Missing font: use default" };

			micro.Font = Font.SystemFontOfSize (NamedSize.Micro);
			small.Font = Font.SystemFontOfSize (NamedSize.Small);
			medium.Font = Font.SystemFontOfSize (NamedSize.Medium);
			large.Font = Font.SystemFontOfSize (NamedSize.Large);

			bold.Font = Font.SystemFontOfSize (NamedSize.Medium, FontAttributes.Bold);
			italic.Font = Font.SystemFontOfSize (NamedSize.Medium, FontAttributes.Italic);
			bolditalic.Font = Font.SystemFontOfSize (NamedSize.Medium, FontAttributes.Bold | FontAttributes.Italic);

			var fontName = Device.OnPlatform ("Georgia", "sans-serif-light", "Comic Sans MS");
			var font = Font.OfSize (fontName, NamedSize.Medium);
			customFont.Font = font;
			italicfont.Font = font.WithAttributes (FontAttributes.Italic);
			boldfont.Font = font.WithAttributes (FontAttributes.Bold);
			bolditalicfont.Font = font.WithAttributes (FontAttributes.Bold | FontAttributes.Italic);

			customFont.GestureRecognizers.Add (new TapGestureRecognizer{Command = new Command (o => customFont.Font = Font.Default)});

			missingfont.Font = Font.OfSize ("FooBar", 20);
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
			Device.OnPlatform (iOS: () => {
				if (Device.Idiom == TargetIdiom.Tablet)
					padding = new Thickness (20, 20, 20, 60);
			});

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
