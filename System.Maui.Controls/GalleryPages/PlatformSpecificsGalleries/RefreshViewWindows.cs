using System.Maui.Controls.GalleryPages.RefreshViewGalleries;
using System.Maui.Internals;
using System.Maui.PlatformConfiguration;
using System.Maui.PlatformConfiguration.WindowsSpecific;
using static System.Maui.PlatformConfiguration.WindowsSpecific.RefreshView;
using WindowsOS = System.Maui.PlatformConfiguration.Windows;

namespace System.Maui.Controls.GalleryPages.PlatformSpecificsGalleries
{
	[Preserve(AllMembers = true)]
	public class RefreshViewWindows : ContentPage
	{
		public RefreshViewWindows()
		{
			Title = "Pull To Refresh (from Bottom to Top)";

			Construct();
		}

		void Construct()
		{
			BindingContext = new RefreshViewModel();

			var refreshView = new RefreshView
			{
				BackgroundColor = Color.Red,
				RefreshColor = Color.Yellow
			};

			refreshView.SetBinding(RefreshView.CommandProperty, "RefreshCommand");
			refreshView.SetBinding(RefreshView.IsRefreshingProperty, "IsRefreshing");

			refreshView.On<WindowsOS>().SetRefreshPullDirection(RefreshPullDirection.BottomToTop);

			var listView = new ListView
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var stackLayout = new StackLayout
					{
						Orientation = StackOrientation.Horizontal
					};

					var boxView = new BoxView { WidthRequest = 40 };
					var infoLabel = new Label();

					boxView.SetBinding(BoxView.ColorProperty, "Color");
					infoLabel.SetBinding(Label.TextProperty, "Name");

					stackLayout.Children.Add(boxView);
					stackLayout.Children.Add(infoLabel);

					return new ViewCell { View = stackLayout };
				})
			};
			listView.SetBinding(ListView.ItemsSourceProperty, "Items");

			refreshView.Content = listView;

			Content = refreshView;
		}
	}
}
