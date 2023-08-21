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

using System;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class SpanSetter : ContentView
	{
		readonly CollectionView _cv;
		readonly Entry _entry;

		public SpanSetter(CollectionView cv)
		{
			_cv = cv;

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = "Update" };
			var label = new Label { Text = "Span:", VerticalTextAlignment = TextAlignment.Center };
			_entry = new Entry { Keyboard = Keyboard.Numeric, Text = "3", WidthRequest = 200 };

			layout.Children.Add(label);
			layout.Children.Add(_entry);
			layout.Children.Add(button);

			button.Clicked += UpdateSpan;

			Content = layout;
		}

		public void UpdateSpan()
		{
			if (int.TryParse(_entry.Text, out int span))
			{
				if (_cv.ItemsLayout is GridItemsLayout gridItemsLayout)
				{
					gridItemsLayout.Span = span;
				}
			}
		}

		public void UpdateSpan(object sender, EventArgs e)
		{
			UpdateSpan();
		}
	}
}