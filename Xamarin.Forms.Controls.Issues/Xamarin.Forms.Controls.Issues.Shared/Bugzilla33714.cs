using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Controls.Issues
{

	[Issue (IssueTracker.Bugzilla, 33714, "[WP] Navigating Back Within MasterDetailPage.Detail Causes Crash", NavigationBehavior.PushModalAsync)]
    public class Bugzilla33714 : MasterDetailPage
	{
		public Bugzilla33714 ()
		{
            Master = new MasterPage (this);
            Detail = new NavigationPage (new ContentPage
            {
                Title = "Home",
                Content = new StackLayout
                {
                    Children = {
                        new Label { Text = "This is the home detail page"}
                    }
                }
            });
		}

		public class MoreDetail : ContentPage
		{
			public MoreDetail ()
			{
				Content = new StackLayout {
					Children = {
						new Label { Text = "More details" },
						new Button { Text = "Go to more detail page", Command = new Command(async () => await Navigation.PushAsync(new MoreDetail()))},
						new Button { Text = "Go back", Command = new Command(async () => await Navigation.PopAsync())}
					}
				};
			}
		}

		public class DetailPage : ContentPage
		{
			public DetailPage ()
			{
				Title = "Detail";
				Content = new StackLayout {
					Children = {
						new Label { Text = "This is a Detail ContentPage" },
						new Button { Text = "Go to more detail page", Command = new Command(async () => await Navigation.PushAsync(new MoreDetail()))}
					}
				};
			}
		}

		public class MasterPage : ContentPage
		{
			readonly MasterDetailPage _masterPage;
			List<string> _items;

			public MasterPage (MasterDetailPage masterPage)
			{
				_masterPage = masterPage;
				Title = "Menu";

				for (int i = 0; i < 5; i++ )
				{
					if (i == 0) _items = new List<string>();

					_items.Add("Menu Items");
				}

				var list = new ListView { ItemsSource = _items, RowHeight = 100, HasUnevenRows = true };
				list.ItemSelected += list_ItemSelected;

				Content = list;
			}

			void list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
			{
				//var listView = (ListView) sender;

				_masterPage.Detail = new NavigationPage(new DetailPage());
			}
		}
    }
}
