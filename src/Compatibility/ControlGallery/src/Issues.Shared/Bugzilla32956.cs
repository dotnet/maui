using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 32956, "Setting ListView.SelectedItem to null does not remove list item highlight when list item is tapped multiple times quickly", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Bugzilla32956 : TestNavigationPage
	{
		protected override void Init()
		{
			var list = new List<int>();
			for (var i = 0; i < 10; i++)
				list.Add(i);

			var listView = new ListView
			{
				ItemsSource = list
			};
			listView.ItemSelected += async (sender, args) =>
			{
				if (args.SelectedItem == null)
					return;

				await Task.Delay(1000);
				await Navigation.PushAsync(new ContentPage());
			};

			var contentPage = new ContentPage
			{
				Content = listView
			};
			contentPage.Appearing += (sender, args) =>
			{
				listView.SelectedItem = null;
			};

			PushAsync(contentPage);
		}
	}
}
