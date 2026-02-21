namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21326, "Span does not inherit text styling from Label if that styling is applied using Style", PlatformAffected.All)]
public partial class Issue21326 : ContentPage
{
	public Issue21326()
	{
		var Issue21326resourceDictionary = new ResourceDictionary();

		var headingStyle = new Style(typeof(Label));
		headingStyle.Setters.Add(new Setter { Property = Label.FontFamilyProperty, Value = "MontserratBold" });
		headingStyle.Setters.Add(new Setter { Property = Label.FontSizeProperty, Value = 20.0 });
		Issue21326resourceDictionary.Add("HeadingStyle", headingStyle);

		Resources = Issue21326resourceDictionary;

		// Test 1: FontFamily inheritance (original test)
		var Issue21326testLabel = new Label
		{
			Style = headingStyle,
			AutomationId = "Issue21326Label"
		};

		var formattedString = new FormattedString();
		formattedString.Spans.Add(new Span
		{
			Text = "SHOULD BE MONTSERRATBOLD from Style"
		});
		Issue21326testLabel.FormattedText = formattedString;

		// Test 2: Span with explicit Bold attribute (should render as Bold)
		var boldSpanLabel = new Label
		{
			AutomationId = "BoldSpanLabel"
		};

		var boldFormattedString = new FormattedString();
		boldFormattedString.Spans.Add(new Span
		{
			Text = "This text should be BOLD",
			FontAttributes = FontAttributes.Bold
		});
		boldSpanLabel.FormattedText = boldFormattedString;

		// Test 3: Span with explicit Italic attribute (should render as Italic)
		var italicSpanLabel = new Label
		{
			AutomationId = "ItalicSpanLabel"
		};

		var italicFormattedString = new FormattedString();
		italicFormattedString.Spans.Add(new Span
		{
			Text = "This text should be ITALIC",
			FontAttributes = FontAttributes.Italic
		});
		italicSpanLabel.FormattedText = italicFormattedString;

		// Test 4: Span with Bold+Italic (should render as Bold Italic)
		var boldItalicSpanLabel = new Label
		{
			AutomationId = "BoldItalicSpanLabel"
		};

		var boldItalicFormattedString = new FormattedString();
		boldItalicFormattedString.Spans.Add(new Span
		{
			Text = "This text should be BOLD + ITALIC",
			FontAttributes = FontAttributes.Bold | FontAttributes.Italic
		});
		boldItalicSpanLabel.FormattedText = boldItalicFormattedString;

		Content = new VerticalStackLayout
		{
			Padding = 10,
			Spacing = 20,
			Children =
			{
				new Label { Text = "Test 1: FontFamily inheritance from Style", FontAttributes = FontAttributes.Bold },
				Issue21326testLabel,

				new Label { Text = "Test 2: Span with FontAttributes='Bold'", FontAttributes = FontAttributes.Bold },
				boldSpanLabel,

				new Label { Text = "Test 3: Span with FontAttributes='Italic'", FontAttributes = FontAttributes.Bold },
				italicSpanLabel,

				new Label { Text = "Test 4: Span with Bold + Italic", FontAttributes = FontAttributes.Bold },
				boldItalicSpanLabel,

				new Label
				{
					Text = "If any Span above looks Regular (not Bold/Italic), the FontAttributes are being dropped!",
					TextColor = Colors.Green,
					FontSize = 12
				}
			}
		};
	}
}
