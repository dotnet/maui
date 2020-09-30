using System.Collections.Generic;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	public partial class SwipeItemsDisposeGallery : ContentPage
	{
		public SwipeItemsDisposeGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeItemsDisposeViewModel();
		}
	}

	public class SwipeItemsDisposeModel
	{
		public string Title { get; set; }
		public string SubTitle { get; set; }
	}

	public class SwipeItemsDisposeViewModel : BindableObject
	{
		public SwipeItemsDisposeViewModel()
		{
			Items = new List<SwipeItemsDisposeModel>();
			LoadItems();
		}

		public List<SwipeItemsDisposeModel> Items { get; set; }

		void LoadItems()
		{
			for (int i = 0; i < 10; i++)
			{
				Items.Add(new SwipeItemsDisposeModel
				{
					Title = $"Title {i + 1}",
					SubTitle = $"SubTitle {i + 1}",
				});
			}
		}
	}
}