using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CollectionViewGalleryTestItem
	{
		public DateTime Date { get; set; }
		public string Caption { get; set; }
		public string Image { get; set; }
		public int Index { get; set; }

		public CollectionViewGalleryTestItem(DateTime date, string caption, string image, int index)
		{
			Date = date;
			Caption = caption;
			Image = image;
			Index = index;
		}

		public override string ToString()
		{
			return $"Item: {Index}";
		}
	}
}