using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Gestures Test",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellGestures : TestShell
	{
		const string SwipeTitle = "Swipe";
		const string SwipeGestureSuccess = "SwipeGesture Success";
		const string SwipeGestureSuccessId = "SwipeGestureSuccessId";

		const string TouchListenerTitle = "IOnTouchListener";
		public const string TouchListenerSuccess = "TouchListener Success";
		const string TouchListenerSuccessId = "TouchListenerSuccessId";

		const string TableViewTitle = "Table View";
		const string TableViewId = "TableViewId";

		const string ListViewTitle = "List View";
		const string ListViewId = "ListViewId";

		protected override void Init()
		{
			this.IncreaseFlyoutItemsHeightSoUITestsCanClickOnThem();
			var gesturePage = CreateContentPage(shellItemTitle: SwipeTitle);
			var label = new Label()
			{
				Text = "Swipe Right and Text Should Change to SwipeGestureSuccess",
				AutomationId = SwipeGestureSuccessId
			};

			gesturePage.Content = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "Click through flyout items for all the tests"},
					label
				},
				GestureRecognizers =
				{
					new SwipeGestureRecognizer()
					{
						Direction = SwipeDirection.Right,
						Command = new Command(() =>
						{
							label.Text = SwipeGestureSuccess;
						})
					}
				}
			};

			var webViewPage = CreateContentPage(shellItemTitle: "Webview");
			webViewPage.Content = new StackLayout()
			{
				Children =
				{
					new Label
					{
						Text = "Make sure you can scroll the web page up and down"
					},
					new WebView()
					{
						VerticalOptions = LayoutOptions.FillAndExpand,
						Source = "https://www.xamarin.com"
					}
				}
			};

			var tableViewPage = CreateContentPage(shellItemTitle: TableViewTitle);

			TableView tableView = new TableView() { Intent = TableIntent.Settings, AutomationId = TableViewId };
			TableRoot tableRoot = new TableRoot();
			tableView.Root = tableRoot;

			for (int i = 0; i < 100; i++)
			{
				TableSection tableSection = new TableSection()
				{
					Title = $"section{++i}"
				};
				var text = $"entry{++i}";
				tableSection.Add(new EntryCell() { Label = text, AutomationId = text });
				text = $"entry{++i}";
				tableSection.Add(new EntryCell() { Label = text, AutomationId = text });

				tableRoot.Add(tableSection);
			}
			tableViewPage.Content = tableView;


			var listViewPage = CreateContentPage(shellItemTitle: ListViewTitle);
			ListView listView = new ListView(ListViewCachingStrategy.RecycleElement) { AutomationId = ListViewId };
			listView.ItemsSource = Enumerable.Range(0, 100).Select(x => $"{x} Entry").ToList();
			listViewPage.Content = listView;

			if (DeviceInfo.Platform == DevicePlatform.Android)
			{
				var touchListenter = CreateContentPage(shellItemTitle: TouchListenerTitle);
				touchListenter.Content = new TouchTestView();
			}
		}

		[Preserve(AllMembers = true)]
		public class TouchTestView : ContentView
		{
			public Label Results = new Label() { AutomationId = TouchListenerSuccessId };
			public TouchTestView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						Results
					}
				};

				Results.Text = "Swipe across the screen. This label should change to say Success";
			}
		}

#if UITEST && (__SHELL__)

		[NUnit.Framework.Category(UITestCategories.Gestures)]
		[Test]
		public void SwipeGesture()
		{
			TapInFlyout(SwipeTitle, usingSwipe:true);
			RunningApp.WaitForElement(SwipeGestureSuccessId);
			RunningApp.SwipeLeftToRight(SwipeGestureSuccessId);
			RunningApp.WaitForElement(SwipeGestureSuccess);
		}

		[NUnit.Framework.Category(UITestCategories.TableView)]
		[Test]
		public void TableViewScroll()
		{
			TapInFlyout(TableViewTitle);
			RunningApp.WaitForElement(TableViewId);

			RunningApp.ScrollDownTo("entry30", TableViewId, ScrollStrategy.Gesture, swipePercentage: 0.20, timeout: TimeSpan.FromMinutes(1));
		}

		[NUnit.Framework.Category(UITestCategories.ListView)]
		[Test]
		public void ListViewScroll()
		{
			TapInFlyout(ListViewTitle);
			RunningApp.WaitForElement(ListViewId);
			RunningApp.ScrollDownTo("30 Entry", ListViewId, ScrollStrategy.Gesture, swipePercentage: 0.20, timeout: TimeSpan.FromMinutes(1));
		}

#if __ANDROID__
		[NUnit.Framework.Category(UITestCategories.CustomRenderers)]
		[Test]
		[Compatibility.UITests.FailsOnMauiAndroid]
		public void TouchListener()
		{
			TapInFlyout(TouchListenerTitle);
			RunningApp.WaitForElement(TouchListenerSuccessId);
			RunningApp.SwipeLeftToRight(TouchListenerSuccessId);
			RunningApp.WaitForElement(TouchListenerSuccess);
		}
#endif

#endif
	}
}
