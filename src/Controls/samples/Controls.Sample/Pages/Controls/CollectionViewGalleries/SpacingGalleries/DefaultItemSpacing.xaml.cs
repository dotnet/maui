using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.SpacingGalleries
{
	public partial class DefaultItemSpacing : ContentPage
	{
		public DefaultItemSpacing()
		{
			InitializeComponent();
			Items = new();
			AddItems();
			BindingContext = this;
		}

		public List<string> Items { get; set; }

		void AddItems()
		{
			for (var i = 0; i < 10; i++)
			{
				Items.Add("CollectionView shouldn't have spacing between items");
			}
		}
	}
}