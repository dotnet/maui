using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class ClipToBoundsGallery : ContentPage
	{
		public ClipToBoundsGallery ()
		{
			var child1 = new BoxView { Color = Color.Red };
			var child2 = new BoxView { Color = Color.Blue };
			var button = new Button { Text = "Clip", BackgroundColor = Color.Green };

			Padding = new Thickness (55);
			var layout = new AbsoluteLayout {
				Children = {
					{child1, new Rectangle (-50, 0, 100, 100)},
					{child2, new Rectangle (0, -50, 100, 100)},
					{button, new Rectangle (1.0, 0.5, 100, 100), AbsoluteLayoutFlags.PositionProportional}
				}
			};

			button.Clicked += (sender, args) => layout.IsClippedToBounds = !layout.IsClippedToBounds;

			Content = layout;
		}
	}
}
