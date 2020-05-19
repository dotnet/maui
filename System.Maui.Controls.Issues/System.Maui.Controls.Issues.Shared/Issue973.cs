using System.Diagnostics;

using Xamarin.Forms.CustomAttributes;
using System;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	internal class PageNameObject
	{
		public string PageName { get; private set; }

		public PageNameObject (string pageName)
		{
			PageName = pageName;
		}
	}

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 973, "ActionBar doesn't immediately update when nested TabbedPage is changed", PlatformAffected.Android)]
	public class Issue973 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			var cells = new [] {
				new PageNameObject ("Close Master"),
				new PageNameObject ("Page 1"),
				new PageNameObject ("Page 2"),
				new PageNameObject ("Page 3"),
				new PageNameObject ("Page 4"),
				new PageNameObject ("Page 5"),
				new PageNameObject ("Page 6"),
				new PageNameObject ("Page 7"),
				new PageNameObject ("Page 8"),
			};

			var template = new DataTemplate (typeof (TextCell));
			template.SetBinding (TextCell.TextProperty, "PageName");

			var listView = new ListView { 
				ItemTemplate = template,
				ItemsSource = cells
			};

			listView.BindingContext = cells;

			listView.ItemTapped += (sender, e) => {

				var cellName = ((PageNameObject)e.Item).PageName;

				if (cellName == "Close Master") {
					IsPresented = false;
				} else {
					var d = new CustomDetailPage (cellName) {
						Title = "Detail"
					};

					d.PresentMaster += (s, args) => {
						IsPresented = true;
					};

					Detail = d;
				}
			};

			var master = new ContentPage {
				Padding = new Thickness (0, 20, 0, 0),
				Title = "Master",
				Content = listView
			};

			Master = master;

			var detail = new CustomDetailPage ("Initial Page") {
				Title = "Detail"
			};

			detail.PresentMaster += (sender, e) => {
				IsPresented = true;
			};

			Detail = detail;
		}

#if UITEST
		[Test]
		[Description ("Test tab reset when swapping out detail")]
		[UiTest (typeof(NavigationPage))]
		[UiTest (typeof(TabbedPage))]
		public void Issue973TestsTabResetAfterDetailSwap ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Initial Page Left aligned"));
			RunningApp.WaitForElement (q => q.Marked ("Tab 1"));

			RunningApp.Tap (q => q.Marked ("Tab 2"));
			RunningApp.WaitForElement (q => q.Marked ("Initial Page Right aligned"));
			RunningApp.Screenshot ("Tab 2 showing");

			RunningApp.Tap (q => q.Marked ("Present Master"));

			RunningApp.Tap (q => q.Marked ("Page 4"));
			RunningApp.Screenshot ("Change detail page");

			RunningApp.Tap (q => q.Marked ("Close Master"));

			RunningApp.WaitForElement (q => q.Marked ("Page 4 Left aligned"));
			RunningApp.Screenshot ("Tab 1 Showing and tab 1 should be selected");

			RunningApp.Tap (q => q.Marked ("Tab 2"));
			RunningApp.WaitForElement (q => q.Marked ("Page 4 Right aligned"));
			RunningApp.Screenshot ("Tab 2 showing");
		}
#endif

	}

	internal class CustomDetailPage : TabbedPage
	{
		public event EventHandler PresentMaster;

		public CustomDetailPage (string pageName)
		{
			Title = pageName;

			Children.Add (new ContentPage {
				Title = "Tab 1",
				Content = new StackLayout {
					Children = {
						new Label {
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Start,
							Text = pageName + " Left aligned"
						}
					}
				}
			});

			Children.Add (new ContentPage {
				Title = "Tab 2",
				Content = new StackLayout {
					Children = {
						new Label {
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.End,
							Text = pageName + " Right aligned"
						},
						new Button {
							Text = "Present Master",
							Command = new Command (() => {
								var handler = PresentMaster;
								if (handler != null)
									handler(this, EventArgs.Empty);
							})
						}
					}
				}
			});
		}
	}
}
