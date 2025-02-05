namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 7534, "Span with tail truncation and paragraph breaks with Java.Lang.IndexOutOfBoundsException", PlatformAffected.Android)]
	public partial class Issue7534 : TestContentPage
	{
		protected override void Init()
		{
			var span = new Span
			{
				Text =
				" Mi augue molestie ligula lobortis enim Velit, in. \n Imperdiet eu dignissim odio. Massa erat Hac inceptos facilisis nibh " +
				" Interdum massa Consectetuer risus sociis molestie facilisi enim. Class gravida. \n Gravida sociosqu cras Quam velit, suspendisse" +
				"  leo auctor odio integer primis dui potenti dolor faucibus augue justo morbi ornare sem. "
			};

			var formattedString = new FormattedString();
			formattedString.Spans.Add(span);

			var label = new Microsoft.Maui.Controls.Label
			{
				LineBreakMode = LineBreakMode.TailTruncation,
				VerticalOptions = LayoutOptions.Start,
				FormattedText = formattedString,
				MaxLines = 3
				//max line is less than the text reproduce and textViewExtensions couldn't identify when
				//it's already pass the MaxLines range because of the paragraph('\n' character).
			};

#pragma warning disable CS0618 // Type or member is obsolete
			var labelDescription = new Label
			{
				Text = "If you opened this page, the app didn't crash and you can read three lines in the label above, this test has passed",
				VerticalOptions = LayoutOptions.StartAndExpand,
				AutomationId = "Success"
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var layout = new Microsoft.Maui.Controls.StackLayout();
			layout.Children.Add(label);
			layout.Children.Add(labelDescription);

			Content = layout;
		}
	}
}
