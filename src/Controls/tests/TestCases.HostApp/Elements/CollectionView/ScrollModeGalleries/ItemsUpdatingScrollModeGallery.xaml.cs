﻿using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.CollectionViewGalleries.ScrollModeGalleries
{
	public class ItemsUpdatingScrollModeItem
	{
		public string Text1 { get; set; }
		public string Text2 { get; set; }
	}

	public class ItemsUpdatingScrollModeViewModel : BindableObject
	{
		public ObservableCollection<ItemsUpdatingScrollModeItem> Items { get; private set; } = new ObservableCollection<ItemsUpdatingScrollModeItem>();

		public async Task LoadItemsAsync()
		{
			for (int i = 0; i < 50; i++)
			{
				Items.Add(new ItemsUpdatingScrollModeItem
				{
					Text1 = $"Title {i + 1}",
					Text2 = $"Subtitle {i + 1}",
				});

				await Task.Delay(1000);
			}
		}
	}

	public partial class ItemsUpdatingScrollModeGallery : ContentPage
	{
		ItemsUpdatingScrollModeViewModel _viewModel;
		public ItemsUpdatingScrollModeGallery()
		{
			InitializeComponent();

			BindingContext = _viewModel = new ItemsUpdatingScrollModeViewModel();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			await _viewModel.LoadItemsAsync();
		}

		void OnItemsUpdatingScrollModeChanged(object sender, EventArgs e)
		{
			CollectionView.ItemsUpdatingScrollMode = (ItemsUpdatingScrollMode)(sender! as EnumPicker)!.SelectedItem;
		}
	}
}