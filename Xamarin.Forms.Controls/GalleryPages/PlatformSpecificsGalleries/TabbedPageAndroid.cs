using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformSpecificsGalleries
{
	public class TabbedPageAndroid : TabbedPage
	{
		public TabbedPageAndroid(ICommand restore)
		{
			Children.Add(CreateFirstPage(restore));
			Children.Add(CreateAdditonalPage());
			Children.Add(CreateAdditonalPage());
			Children.Add(CreateAdditonalPage());
			Children.Add(CreateAdditonalPage());
			On<Android>().SetOffscreenPageLimit(2);
		}

		ContentPage CreateFirstPage(ICommand restore)
		{
			var page = new ContentPage { Title = "Content Page Title" };
			var offscreenPageLimit = new Label();
			var content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children =
				{
					new Button
					{
						Text = "Click to Toggle Swipe Paging",
						Command = new Command(() => On<Android>().SetIsSwipePagingEnabled(!On<Android>().IsSwipePagingEnabled()))
					},
					new Button
					{
						Text = "Click to Toggle Smooth Scroll",
						Command = new Command(() => On<Android>().SetIsSmoothScrollEnabled(!On<Android>().IsSmoothScrollEnabled()))
					},
					offscreenPageLimit
				}
			};

			var restoreButton = new Button { Text = "Back To Gallery" };
			restoreButton.Clicked += (sender, args) => restore.Execute(null);
			content.Children.Add(restoreButton);

			page.Content = content;

			return page;
		}

		static Page CreateAdditonalPage()
		{
			var cp = new ContentPage { Title = "Additional Page" };

			cp.Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children =
				{
					new Entry
					{
						Placeholder = "Enter some text"
					}
				}
			};

			return cp;
		}
	}
}
