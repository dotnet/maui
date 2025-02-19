using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1703, "Memory leak when navigating a page off of a navigation stack", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public class Issue1703 : NavigationPage
	{
		static List<WeakReference> s_pageRefs = new List<WeakReference>();

		public Issue1703()
		{
			Navigation.PushAsync(GetMainPage());
		}

		public static Page GetMainPage()
		{
			return CreateWeakReferencedPage();
		}

		static Page CreateWeakReferencedPage()
		{
			GarbageCollectionHelper.Collect();
			var result = CreatePage();
			s_pageRefs.Add(new WeakReference(result));

			// Add a second unreferenced page to prove that the problem only exists
			// when pages are actually navigated to/from
			s_pageRefs.Add(new WeakReference(CreatePage()));
			GarbageCollectionHelper.Collect();
			return result;
		}

		static Page CreatePage()
		{
			var page = new ContentPage();
			var contents = new StackLayout();

			contents.Children.Add(
				new Button
				{
					Text = "Next Page",
					Command = new Command(() => page.Navigation.PushAsync(CreateWeakReferencedPage()))
				});
			contents.Children.Add(
				new Label
				{
					Text = string.Format(
						"References alive at time of creation: {0}",
						s_pageRefs.Count(p => p.IsAlive)),
					HorizontalOptions = LayoutOptions.CenterAndExpand
				});

			page.Content = contents;
			return page;
		}
	}
}

