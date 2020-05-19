using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44500,
		"A WebView that has a file picker control fails to show photo picker when page is pushed modally.",
		PlatformAffected.iOS)]
	public class Bugzilla44500 : TestNavigationPage
	{
		const string Html = @"
<html>
	<head>
		<title></title>
	</head>
	<body>
		<form>
			<p>Please select a file:<br>
				<input type=""file"" name=""datafile"" size=""40"">
			</p>
		</form>
	</body>
</html>";

		protected override async void Init()
		{
			// If you run this test and see a ton of errors like this in your console:
			// 2017-05-31 17:09:17.662 XamarinFormsControlGalleryiOS[933:703025] AX Exchange error: Error Domain=Accessibility Code=0 "Remote service does not respond to _accessibilityMachPort" UserInfo={NSLocalizedDescription=Remote service does not respond to _accessibilityMachPort}
			// That's just a Calabash bug, you can safely ignore it. 
			// If you want to avoid having your console spammed, remove 'Xamarin.Calabash.Start();' from your application. 

			var instructions = new Label
			{
				Text = "Click the 'Choose file' button in the WebView. Select 'Photo Library'. If the Photos screen displays, this test has passed. (You may have to give the app permission to open Photos first.)"
			};

			var showModal = new Button { Text = "Tap Here" };
			var root = new ContentPage { Content = showModal };

			var htmlSource = new HtmlWebViewSource { Html = Html };

			var modalContent = new ContentPage
			{
				Content = new StackLayout
				{
					Margin = new Thickness(40),
					VerticalOptions = LayoutOptions.Fill,
					Children =
					{
						instructions,
						new WebView { HeightRequest = 200, Source = htmlSource }
					}
				}
			};

			showModal.Clicked += (sender, args) => { Navigation.PushModalAsync(modalContent); };

			await PushAsync(root);
		}
	}
}