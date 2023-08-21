//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System.Collections.Generic;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
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