﻿using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	public record OnlineImageInfo(string Uri);

	public partial class OnlineImages : ContentPage
	{
		public OnlineImages()
		{
			InitializeComponent();

			Dispatcher.DispatchAsync(SetItemsSource);
		}

		async Task SetItemsSource()
		{
			await Task.Delay(TimeSpan.FromSeconds(1));

			CollectionView.ItemsSource = new ObservableCollection<OnlineImageInfo>
			{
				new ("https://news.microsoft.com/wp-content/uploads/prod/2022/07/hexagon_print.gif"),
				new ("https://news.microsoft.com/wp-content/uploads/prod/2022/07/collaboration-controls-in-power-platform_print.gif"),
				new ("https://news.microsoft.com/wp-content/uploads/prod/2022/07/Updatesin-Teams.png"),
				new ("https://news.microsoft.com/wp-content/uploads/prod/2022/07/Expanded-Reactions.png"),
				new ("https://news.microsoft.com/wp-content/uploads/prod/2022/04/Companion-Devices.jpg"),
			};
		}
	}
}