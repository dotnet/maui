using System;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1700, "Image fails loading from long URL", PlatformAffected.iOS | PlatformAffected.Android | PlatformAffected.WinPhone)]
	public class Issue1700 : ContentPage
	{
		public Issue1700 ()
		{
			var stack = new StackLayout();
			var url = "http://imgs.xkcd.com/comics/tasks.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb";
			var url2 = "http://www.optipess.com/wp-content/uploads/2010/08/02_Bad-Comics6-10.png?a=bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbasdasdasdasdasasdasdasdasdasd";
			var img = new Image{  
				Source = new UriImageSource { Uri =  new Uri(url) }
			} ;
			stack.Children.Add(img);
			var img2 = new Image{  
				Source = new UriImageSource { Uri =  new Uri(url2) }
			} ;
			stack.Children.Add(img2);
			Content = new ScrollView() { Content =  stack };
		}
	}
}


