using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.ListViewGalleries
{
	public class ListViewViewCell : ContentPage
	{
		public ListViewViewCell()
		{
			Content = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					var cell = new ViewCell();
					var label = new Label();
					var image = new Image();

					var hl = new HorizontalStackLayout()
					{
						Children =
						{
							label,
							image
						}
					};

					label.SetBinding(Label.TextProperty, "Text");
					image.SetBinding(Image.SourceProperty, "Image");

					cell.View = hl;
					return cell;
				}),

				ItemsSource = Enumerable.Range(0, 100)
					.Select(i => (i % 2 == 0) ? new { Text = "Coffee: ", Image = "coffee.png" } : new { Text = "Vegetables: ", Image = "vegetables.jpg" })
					.ToList()
			};
		}
	}
}
