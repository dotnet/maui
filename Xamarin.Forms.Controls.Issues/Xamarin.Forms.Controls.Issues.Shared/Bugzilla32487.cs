using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 32487, " webview in tabbedpage has black margin")]
	public class Bugzilla32487 : TestTabbedPage
	{
		protected override void Init ()
		{
			var cp = new ContentPage ();
			cp.Title = "bugzila 30047";
			Children.Add (cp);

			var cp1 = new ContentPage ();
			cp1.Title = "bugzila 32487";
			var sl = new StackLayout ();
			var wv = new WebView ();

			var htmlSource = new HtmlWebViewSource ();
			htmlSource.Html = "<h3>Welcome to the real-time HTML editor!</h3>\n<p>Try scroll this page, you will see black margins if it isn't working ok</p>";
			wv.Source = htmlSource;
			sl.Children.Add (wv);
			cp1.Content = wv;

			var btn = new Button { Text = "tap and rotate device after ", Command = new Command (async () => {
					ContentPage cp2 = new ContentPage ();
					cp.Title = "rotation";
					var grd = new Grid ();
					grd.RowDefinitions.Add (new RowDefinition { Height = GridLength.Auto });
					grd.RowDefinitions.Add (new RowDefinition ());
					WebView wv1 = new WebView { Source = "http://xamarin.com" };
					Grid.SetRow (wv1, 1);
					grd.Children.Add (wv1);
					grd.Children.Add (new Button {
						Text = "Back",
						BackgroundColor = Color.Red,
						Command = new Command (() => Navigation.PopModalAsync ())
					});
					cp2.Content = grd;
					await Navigation.PushModalAsync (cp2);
				})
			};
			cp.Content = btn;

			Children.Add (cp);
			Children.Add (cp1);
		}
	}
}
