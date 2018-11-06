using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1705, "Editor.IsEnabled = false", PlatformAffected.iOS)]
	public class Issue1705 : ContentPage
	{
		public Issue1705 ()
		{
			Title = "test page";
			Padding = 10;

			var layout = new StackLayout ();
			var t = new Entry {
				IsEnabled = false
			};

			var e = new Editor {
				IsEnabled = false,
				BackgroundColor = Color.Aqua
			};

			layout.Children.Add (e);
			layout.Children.Add (t);

			Content = layout;
		}
	}

}
