using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36651, "ScrollToAsync fails to scroll on Windows Phone 8.1 RT", PlatformAffected.WinRT)]
	public class Bugzilla36651 : TestNavigationPage
	{
		public class LandingPage : ContentPage
		{
			private static int numberOfButtons = 3;
			StackLayout layout;
			Button addItem, removeItem;
			ScrollView scrollView;

			public LandingPage()
			{
				layout = new StackLayout();
				for (var i = 0; i < 10; i++)
				{
					layout.Children.Add(new Button { Text = "This is a button" });
				}

				addItem = new Button { Text = "Add Item" };
				removeItem = new Button { Text = "Remove Item" };
				scrollView = new ScrollView { Content = layout };

				Content = new StackLayout
				{
					Children =
					{
						new Label
						{
							Text = "Clicking 'Add Item' should scroll to the end when added items go out of the view"
						},
						scrollView,
						addItem,
						removeItem
					}
				};

				addItem.Clicked += (object sender, EventArgs e) =>
				{
					Button lastButton = null;
					for (int i = 0; i < numberOfButtons; ++i)
					{
						lastButton = new Button
						{
							Text = String.Format("This is button #{0}", i)
						};
						layout.Children.Add(lastButton);
					}
					++numberOfButtons;

					scrollView.ScrollToAsync(lastButton, ScrollToPosition.End, false);
				};
				removeItem.Clicked += (object sender, EventArgs e) =>
				{
					if (layout.Children.Count != 0)
						layout.Children.RemoveAt(layout.Children.Count - 1);
				};
			}
		}

		protected override void Init()
		{
			var page = new LandingPage();
			Navigation.PushAsync(page);
		}
	}
}