using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.NestedGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class NestedCollectionViewGallery : ContentPage
	{
		public NestedCollectionViewGallery()
		{
			InitializeComponent();
			BindingContext = new NestedCollectionViewModel();
		}
	}

	[Preserve(AllMembers = true)]
	internal class NestedItemSource
	{
		public List<CollectionViewGalleryTestItem> Items { get; set; }
		public string Title { get; set; }

		public NestedItemSource(string title)
		{
			Items = new List<CollectionViewGalleryTestItem>();

			int count = new Random().Next(6, 15);

			var source = new DemoFilteredItemSource(count);

			for (int n = 0; n < count; n++)
			{
				Items.Add(source.Items[n]);
			}

			Title = title;
		}
	}

	[Preserve(AllMembers = true)]
	internal class NestedCollectionViewModel
	{
		public List<NestedItemSource> Items { get; set; }

		public NestedCollectionViewModel()
		{
			Items = new List<NestedItemSource>();

			for (int n = 0; n < 20; n++)
			{
				Items.Add(new NestedItemSource($"Source {n}"));
			}
		}
	}
}