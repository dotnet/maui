using System;
using System.Collections.ObjectModel;
using System.Linq;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using System.Diagnostics;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Ignore("Temporarily ignoring until we can investigate intermittent failures")]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 1691, "CarouselPage iOS CurrentPage bug")]
	public class Issue1691 : TestCarouselPage
	{
		int _currentIndex;
		int _page = 9;

		protected override void Init ()
		{
			_currentIndex = 10;
			ItemsSource = new ObservableCollection<int>() { _currentIndex };
			SelectedItem = ((ObservableCollection<int>)ItemsSource)[0];
		}

		protected override ContentPage CreateDefault (object item)
		{
			var currentInt = item as int?;

			var label = new Label {
				Text = "Page " + currentInt,
			}; 

			return new ContentPage {
				Content = new StackLayout {
					Children = {
						label, 
						new Button {
							AutomationId = "CreatePreviousPage" + currentInt,
							Text = "Create previous page",
							Command = new Command (() => {
								((ObservableCollection<int>)ItemsSource).Insert (0, _page);
								_page--;
								label.Text = "Page Created";
							})
						},
						new Button {
							AutomationId = "GoToPreviousPage" + currentInt,
							Text = "Go to previous page",
							Command = new Command (() => {
								CurrentPage = Children[0];
							})
						}
					}
				}
			};
		}

#if UITEST
		[Test]
		public void Issue1691Test ()
		{
			RunningApp.Screenshot ("I am at Issue 1691");
			RunningApp.Tap (q => q.Marked ("CreatePreviousPage10"));
			RunningApp.WaitForElement (q => q.Marked ("Page Created"));
			RunningApp.Screenshot ("I should be on the same page with a new page created to the left");
			RunningApp.Tap (q => q.Marked ("GoToPreviousPage10"));
			RunningApp.WaitForNoElement (q => q.Marked ("GoToPreviousPage10"));
			RunningApp.Screenshot ("I should be on page 9");
			RunningApp.WaitForElement (q => q.Marked ("GoToPreviousPage9"));
		}
#endif
	}
}
