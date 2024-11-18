using System.ComponentModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Entry = Microsoft.Maui.Controls.Entry;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 25836, "Span with tail truncation and paragraph breaks with exception", PlatformAffected.Android)]
	public class Issue25836 : ContentPage
	{
		public Issue25836()
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

			var label = new Label
			{
				AutomationId = "Label",
				LineBreakMode = LineBreakMode.TailTruncation,
				VerticalOptions = LayoutOptions.Start,
				FormattedText = formattedString,
				MaxLines = 3
			};

			var layout = new StackLayout();
			layout.Children.Add(label);

			Content = layout;
		}
	}
}
