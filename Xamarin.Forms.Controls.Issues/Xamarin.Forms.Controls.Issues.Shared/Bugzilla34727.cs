using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 34727, "(A) Cannot browse files from WebView on Android", PlatformAffected.Android)]
	public class Bugzilla34727 : TestContentPage
	{
		protected override void Init ()
		{
			var webView = new WebView ();

			var htmlSource = new HtmlWebViewSource { Html = @"
<h3>Test Web View File Chooser</h3>
<ol>
	<li>Open the camera app.</li>
	<li>Take a picture.</li>
	<li>Return to this page.</li>
	<li>Tap the 'Choose File' button; a file picker should appear.</li>
	<li>Select the picture you just took.</li>
	<li>The text 'No file chosen' should change to the name of your image file.</li>
</ol>
<br/>
<input type='file' name='file' id='chooser' accept='image/*'>" };

			webView.Source = htmlSource;
			Content = webView;
		}
	}
}
