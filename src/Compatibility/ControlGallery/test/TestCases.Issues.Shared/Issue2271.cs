using System;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2271, "ScrollToAsync not working on iOS", PlatformAffected.iOS)]
	public class Issue2271 : TestNavigationPage
	{
		public class LandingPage : ContentPage
		{
			StackLayout layout;
			Button addAtEndAndScrollToEnd, addAtStartAndScrollToStart, scrollToStart, scrollToEnd;
			ScrollView scrollView;

			public LandingPage()
			{
				layout = new StackLayout();
				for (var i = 0; i < 100; i++)
				{
					layout.Children.Add(new Label { Text = $"This is a button {layout.Children.Count}" });
				}

				addAtEndAndScrollToEnd = new Button { Text = "Add Item and scroll to bottom" };
				addAtStartAndScrollToStart = new Button { Text = "Add item at beginning and move to beginning" };
				scrollToStart = new Button { Text = "Scroll to first" };
				scrollToEnd = new Button { Text = "Scroll to last" };
				scrollView = new ScrollView { Content = layout };

				Content = new StackLayout
				{
					Children =
					{
						scrollView,
						addAtEndAndScrollToEnd,
						addAtStartAndScrollToStart,
						scrollToStart,
						scrollToEnd
					}
				};

				addAtEndAndScrollToEnd.Clicked += AddItem_Clicked;
				addAtStartAndScrollToStart.Clicked += AddItemAtBegging_Clicked;
				scrollToEnd.Clicked += ScrollToEnd_Clicked;
				scrollToStart.Clicked += ScrollToStart_Clicked;
			}

			async void ScrollToStart_Clicked(object sender, EventArgs e)
			{
				await scrollView.ScrollToAsync((Element)layout.First(), ScrollToPosition.Start, false);
			}

			async void ScrollToEnd_Clicked(object sender, EventArgs e)
			{
				await scrollView.ScrollToAsync((Element)layout.Last(), ScrollToPosition.End, false);
			}

			async void AddItem_Clicked(object sender, EventArgs e)
			{
				Label lastButton = null;
				for (int i = 0; i < 10; ++i)
				{
					lastButton = new Label
					{
						Text = $"Insert nr {layout.Children.Count}"
					};
					layout.Children.Add(lastButton);
				}

				await scrollView.ScrollToAsync(lastButton, ScrollToPosition.End, false);
			}

			async void AddItemAtBegging_Clicked(object sender, EventArgs e)
			{
				Label lastButton = null;
				for (int i = 0; i < 10; ++i)
				{
					lastButton = new Label
					{
						Text = $"Insert nr {layout.Children.Count}"
					};
					layout.Children.Insert(0, lastButton);
				}

				await scrollView.ScrollToAsync(lastButton, ScrollToPosition.Start, false);
			}
		}

		protected override void Init()
		{
			var page = new LandingPage();
			Navigation.PushAsync(page);
		}
	}
}
