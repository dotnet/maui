using System;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 38105, "RemovePage does not cause back arrow to go away on Android",
		NavigationBehavior.PushModalAsync)]
	internal class Bugzilla38105 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			Detail = new NavigationPage (new ViewA ());

			var button = new Button () { Text = "Click me" };
			button.Clicked += (o, e) => {
				var navPage = (NavigationPage)Detail;

				var rootPage = navPage.CurrentPage;

				navPage.PopToRootAsync (false);

				navPage.Navigation.PushAsync (new ViewB ());

				navPage.Navigation.RemovePage (rootPage);

				IsPresented = false;
			};

			Master = new ContentPage () {
				Title = "test",
				Content = button
			};
		}

		public class ViewA : ContentPage
		{
			public ViewA ()
			{
				Title = "View A";
			}
		}

		public class ViewB : ContentPage
		{
			public ViewB ()
			{
				Title = "View B";
			}
		}
	}
}

