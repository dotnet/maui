using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class UnevenViewCellGallery : ContentPage
	{
		public UnevenViewCellGallery ()
		{
			Title = "UnevenViewCell Gallery - Legacy";

			var map = MapGallery.MakeMap ();
			map.HasScrollEnabled = false;

			Content = new TableView {
				RowHeight = 150,
				HasUnevenRows = true,
				Root = new TableRoot {
					new TableSection ("Testing") {
						new ViewCell {View = map, Height = 250},
						new ViewCell {View = new ProductCellView ("1 day")},
						new ViewCell {View = new ProductCellView ("2 days")},
						new ViewCell {View = new ProductCellView ("3 days")},
						new ViewCell {View = new ProductCellView ("4 days")},
						new ViewCell {View = new ProductCellView ("5 days")}
					}
				}
			};
		}
	}
}
