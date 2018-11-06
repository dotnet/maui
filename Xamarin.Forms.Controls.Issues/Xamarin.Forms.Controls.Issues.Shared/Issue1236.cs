using System;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1236, "Label binding", PlatformAffected.iOS)]
	public class Issue1236 : ContentPage
	{
		public Issue1236 ()
		{
			Content = new Label { HeightRequest = 30, WidthRequest = 200, BackgroundColor = Color.Purple.WithLuminosity (.7) };
			Content.SetBinding (Label.TextProperty, ".");

			DelayUpdatingBindingContext ();
		}

		async void DelayUpdatingBindingContext ()
		{
			await Task.Delay (2000);
			BindingContext = "Lorem Ipsum Dolor Sit Amet";
		}
	}
}
