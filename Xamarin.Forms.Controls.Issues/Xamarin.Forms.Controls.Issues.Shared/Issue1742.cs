using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1742, "Invisible Button still fires Clicked event", PlatformAffected.WinPhone)]
	public class Issue1742 : ContentPage
	{
		public Issue1742 ()
		{
			 var listView = new ListView
            {
                RowHeight = 40
            };
            var invisibleButton = new Button
            {
                IsVisible = false,
                Text = "INVISIBLE button"
            };
            var visibleButton = new Button
            {
                IsVisible = true,
                Text = "Visible button"
            };

            invisibleButton.Clicked += Button_Clicked;
            visibleButton.Clicked += Button_Clicked;
            listView.ItemTapped += ListView_ItemTapped;

            listView.ItemsSource = new string[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" };

			Content = new StackLayout {
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children = { listView, visibleButton, invisibleButton }
			};

		}

		void ListView_ItemTapped(object sender, ItemTappedEventArgs args)
        {
            DisplayAlert("Alert", "List item tapped", "OK", "Cancel");
        }

		void Button_Clicked(object sender, EventArgs args)
        {
            DisplayAlert("Alert", ((Button)sender).Text + " clicked", "OK", "Cancel");
        }
	}
}
