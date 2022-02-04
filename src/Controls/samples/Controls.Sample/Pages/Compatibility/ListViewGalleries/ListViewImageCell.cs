using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ListViewGalleries
{
	public class ListViewImageCell : ContentPage
	{
		public ListViewImageCell()
		{
			Content = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new ImageCell();
					cell.SetBinding(ImageCell.ImageSourceProperty, ".");
					return cell;
				}),

				ItemsSource = Enumerable.Range(0, 100)
					.Select(i => (i % 2 == 0) ? "coffee.png" : "vegetables.jpg")
					.ToList()
			};
		}
	}
}
