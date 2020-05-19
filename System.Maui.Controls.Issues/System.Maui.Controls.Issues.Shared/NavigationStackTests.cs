using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0, "Navigation Backstack tests", PlatformAffected.All)]
	public class NavigationStackTests : ContentPage
	{
		static int s_pageNumber = 0;

		public NavigationStackTests ()
		{
			Title = "Navigation Page " + s_pageNumber;

			var label = new Label {Text = Title};
			var pushBtn = new Button {Text = "Push"};
			var pushModalBtn = new Button {Text = "Push Modal"};
			var pushNoAnimBtn = new Button {Text = "Push No Animation"};
			var pushModalNoAnimBtn = new Button {Text = "Push Modal No Animation"};
			var popBtn = new Button {Text = "Pop"};
			var popModalBtn = new Button {Text = "Pop Modal"};
			var popNoAnimBtn = new Button {Text = "Pop No Animation"};
			var popModalNoAnimBtn = new Button {Text = "Pop Modal No Animation"};
			var insertPageBtn = new Button {Text = "Insert Page"};
			var removePageBtn = new Button {Text = "Remove Page"};

			pushBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PushAsync (new NavigationStackTests ());

			pushModalBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PushModalAsync (new NavigationPage(new NavigationStackTests ()));

			pushNoAnimBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PushAsync (new NavigationStackTests (), false);

			pushModalNoAnimBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PushModalAsync (new NavigationPage(new NavigationStackTests ()), false);

			popBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PopAsync ();

			popModalBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PopModalAsync ();

			popNoAnimBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PopAsync (false);

			popModalNoAnimBtn.Clicked += async (sender, args) => await ((Button) sender).Navigation.PopModalAsync (false);

			insertPageBtn.Clicked += (sender, args) => ((Button) sender).Navigation.InsertPageBefore (new NavigationStackTests (), this);

			removePageBtn.Clicked += (sender, args) => {
				Element parent = ((Element) sender);
				NavigationPage navPage = null;
				while (navPage == null && parent != null) {
					navPage = parent as NavigationPage;
					parent = parent.Parent;
				}

				if (navPage != null) {
					navPage.Navigation.RemovePage ((Page)((IPageController)navPage).InternalChildren[((IPageController)navPage).InternalChildren.Count - 2]);
				}
			};

			Content = new ScrollView {
				Content = new StackLayout {
					Children = {
						label,
						pushBtn,
						pushModalBtn,
						pushNoAnimBtn,
						pushModalNoAnimBtn,
						popBtn,
						popModalBtn,
						popNoAnimBtn,
						popModalNoAnimBtn,
						insertPageBtn,
						removePageBtn
					}
				}
			};

			s_pageNumber++;
		}
	}
}
