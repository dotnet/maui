using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
    [Preserve(AllMembers = true)]
    public class UnevenListGallery : ContentPage
	{
        public UnevenListGallery ()
		{
			Padding = new Thickness (0, 20, 0, 0);

			var list = new ListView {
				HasUnevenRows = true
			};

			bool next = true;
			list.ItemTemplate = new DataTemplate (() => {
				bool tall = next;
				next = !next;
				
				var cell = new TextCell {
					Height = (tall) ? 88 : 44
				};

				cell.SetBinding (TextCell.TextProperty, ".");
				return cell;
			});

			list.ItemsSource = new[] { "Tall", "Short", "Tall", "Short" };

			var listViewCellDynamicHeight = new ListView {
				HasUnevenRows = true,
				AutomationId= "unevenCellListGalleryDynamic"
			};

			listViewCellDynamicHeight.ItemsSource = new [] { 
				@"That Flesh is heir to? 'Tis a consummation
Devoutly to be wished. To die, to sleep,
To sleep, perchance to Dream; Aye, there's the rub,
For in that sleep of death, what dreams may come,That Flesh is heir to? 'Tis a consummation
Devoutly to be wished. To die, to sleep,
To sleep, perchance to Dream; Aye, there's the rub,
For in that sleep of death, what dreams may come",
			};

			listViewCellDynamicHeight.ItemTemplate = new DataTemplate (typeof(UnevenRowsCell));

			listViewCellDynamicHeight.ItemTapped += (sender, e) => {
				if (e == null)
					return; // has been set to null, do not 'process' tapped event
				((ListView)sender).SelectedItem = null; // de-select the row
			};

			var grd = new Grid ();

			grd.RowDefinitions.Add (new RowDefinition ());
			grd.RowDefinitions.Add (new RowDefinition ());
		
			grd.Children.Add (listViewCellDynamicHeight);
			grd.Children.Add (list);
		
			Grid.SetRow (list, 1);
		
			Content =  grd;
		}


        [Preserve(AllMembers = true)]
        public class UnevenRowsCell : ViewCell
		{
			public UnevenRowsCell ()
			{
				var label1 = new Label {
					Text = "Label 1",
					FontSize = Device.GetNamedSize (NamedSize.Small, typeof(Label))
				};
				label1.SetBinding (Label.TextProperty, new Binding ("."));

				View = new StackLayout {
					Orientation = StackOrientation.Vertical,
					VerticalOptions = LayoutOptions.StartAndExpand,
					Padding = new Thickness (15, 5, 5, 5),
					Children = { label1 }
				};
			}
			// This is the code used before and still works by setting the height of the cell
			//		const int avgCharsInRow = 35;
			//		const int defaultHeight = 44;
			//		const int extraLineHeight = 20;
			//		protected override void OnBindingContextChanged ()
			//		{
			//			base.OnBindingContextChanged ();
			//
			//			if (Device.OS == TargetPlatform.iOS) {
			//				var text = (string)BindingContext;
			//
			//				var len = text.Length;
			//
			//				if (len < (avgCharsInRow * 2)) {
			//					// fits in one cell
			//					Height = defaultHeight;
			//				} else {
			//					len = len - (avgCharsInRow * 2);
			//					var extraRows = len / 35;
			//					Height = defaultHeight + extraRows * extraLineHeight;
			//				}
			//			}
			//		}
		}
	}
}
