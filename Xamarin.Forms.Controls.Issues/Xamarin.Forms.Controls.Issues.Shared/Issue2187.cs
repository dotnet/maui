using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2187, "[WPF] FontFamily Assignment in platform specific project don't work", PlatformAffected.WPF)]
	public class Issue2187 : TestContentPage
	{
		protected override void Init()
		{
			var stack = new StackLayout();
			Label issueTestLabel1Description = new Label();
			issueTestLabel1Description.Text = "Below is supposed to show a hamburger icon (like used in a mobile app)" +
				" from the FontAwesome font (in the current project with namespace) :\nFontFamily = \"/Xamarin.Forms.ControlGallery.WPF;component/Assets/#FontAwesome\"";


			Label issueTestLabel1 = new Label
			{
				TextColor = Color.Red,
				HorizontalTextAlignment = TextAlignment.Center,
				FontSize = 40,
				Text = "\uf0c9"
			};

			switch (Device.RuntimePlatform)
			{
				case Device.WPF:
					issueTestLabel1.FontFamily = "/Xamarin.Forms.ControlGallery.WPF;component/Assets/#FontAwesome";
					break;
				default:
					break;
			}

			Label issueTestLabel2Description = new Label();
			issueTestLabel2Description.Text = "Below is supposed to show a umbrella icon (like used in a mobile app)" +
				" from the FontAwesome font (in the nuget package) :\nFontFamily = \"/Meziantou.WpfFontAwesome;component/Resources/#FontAwesome\"";

			Label issueTestLabel2 = new Label
			{
				TextColor = Color.Gray,
				HorizontalTextAlignment = TextAlignment.Center,
				FontSize = 40,
				Text = "\uf0e9"
			};

			switch (Device.RuntimePlatform)
			{
				case Device.WPF:
					issueTestLabel2.FontFamily = "/Meziantou.WpfFontAwesome;component/Resources/#FontAwesome";
					break;
				default:
					break;
			}

			Label issueTestLabel3Description = new Label();
			issueTestLabel3Description.Text = "Below is supposed to show a text from the Pick Ax font " +
				" (in the current project without namespace) :\nFontFamily = \"/Assets/#Pick Ax\"";


			Label issueTestLabel3 = new Label
			{
				TextColor = Color.Red,
				HorizontalTextAlignment = TextAlignment.Center,
				FontSize = 40,
				Text = "Hello, I'm in Pick Ax font"
			};

			switch (Device.RuntimePlatform)
			{
				case Device.WPF:
					issueTestLabel3.FontFamily = "/Assets/#Pick Ax";
					break;
				default:
					break;
			}

			stack.Children.Add(issueTestLabel1Description);
			stack.Children.Add(issueTestLabel1);
			stack.Children.Add(issueTestLabel2Description);
			stack.Children.Add(issueTestLabel2);
			stack.Children.Add(issueTestLabel3Description);
			stack.Children.Add(issueTestLabel3);

			Label issueTestSpan1Description = new Label
			{
				Text = "Span with font"
			};

			Label issueTestSpan1 = new Label()
			{
				HorizontalTextAlignment = TextAlignment.Center,
			};
			
			Span span = new Span()
			{
				TextColor = Color.Red,
				FontSize = 40,
				Text = "Hello, I'm in Pick Ax font"
			};

			switch (Device.RuntimePlatform)
			{
				case Device.WPF:
					span.FontFamily = "/Assets/#Pick Ax";
					break;
				default:
					break;
			}

			Span span1 = new Span()
			{
				TextColor = Color.Blue,
				FontSize = 30,
				Text = " - without font but blue"
			};

			issueTestSpan1.FormattedText = new FormattedString();
			issueTestSpan1.FormattedText.Spans.Add(span);
			issueTestSpan1.FormattedText.Spans.Add(span1);
			stack.Children.Add(issueTestSpan1Description);
			stack.Children.Add(issueTestSpan1);

			Label issueTestButton1Description = new Label
			{
				Text = "Button with font"
			};

			Button issueTestButton1 = new Button
			{
				TextColor = Color.Red,
				FontSize = 40,
				Text = "Hello, I'm in Button - Pick Ax font"
			};

			switch (Device.RuntimePlatform)
			{
				case Device.WPF:
					issueTestButton1.FontFamily = "/Assets/#Pick Ax";
					break;
				default:
					break;
			}
			
			stack.Children.Add(issueTestButton1Description);
			stack.Children.Add(issueTestButton1);

			Content = stack;
		}
	}
}
