using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{

	public class CarouselPageGallery : CarouselPage
	{
		public CarouselPageGallery ()
		{
			var pageOneLabel = new Label {
				Text = "No click one"
			};

			var pageTwoLabel = new Label {
				Text = "No click two"
			};

			var pageThreeLabel = new Label {
				Text = "No click three"
			};

			var pageOneButton = new Button {
				Text = "Click me one",
				Command = new Command (() => pageOneLabel.Text = "Clicked one")
			};

			var pageTwoButton = new Button {
				Text = "Click me two",
				Command = new Command (() => pageTwoLabel.Text = "Clicked two")
			};

			var pageThreeButton = new Button {
				Text = "Click me three",
				Command = new Command (() => pageThreeLabel.Text = "Clicked three")
			};

			Children.Add (new ContentPage {
				Title = "Page One",
				BackgroundColor = new Color (1, 0, 0),
				Content = new StackLayout {
					Children = {
						pageOneLabel,
						pageOneButton
					}
				}
			});

			Children.Add (new ContentPage {
				Title = "Page Two",
				BackgroundColor = new Color (0, 1, 0),
				Content = new StackLayout {
					Children = {
						pageTwoLabel,
						pageTwoButton
					}
				}
			});

			Children.Add (new ContentPage {
				Title = "Page Three",
				BackgroundColor = new Color (0, 0, 1),
				Content = new StackLayout {
					Children = {
						pageThreeLabel,
						pageThreeButton
					}
				}
			});
		}
	}
}

