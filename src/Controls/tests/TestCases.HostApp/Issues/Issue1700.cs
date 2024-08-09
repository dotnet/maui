using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1700, "Image fails loading from long URL", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1700 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var stack = new StackLayout();
			var url = "https://github.com/xamarin/Xamarin.Forms/raw/main/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/Legumes.jpg?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
			var url2 = "https://github.com/xamarin/Xamarin.Forms/raw/main/Microsoft.Maui.Controls.ControlGallery.Android/Resources/drawable/Vegetables.jpg?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd";
			var img = new Image
			{
				Source = new UriImageSource { Uri = new Uri(url) }
			};
			stack.Children.Add(img);
			var img2 = new Image
			{
				Source = new UriImageSource { Uri = new Uri(url2) }
			};
			stack.Children.Add(img2);

			var success = new Label { Text = Success };
			stack.Children.Add(success);

			Content = new ScrollView() { Content = stack };
		}
	}
}


