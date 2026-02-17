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

		Content = new VerticalStackLayout
		{
			Padding = 10,
			Spacing = 20,
			Children =
			{
				new Label { Text = "Test Passes if below label rendered in MontserratBold font from Style" },
				Issue21326testLabel
			}
		};
	}
}
