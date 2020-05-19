using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class MultiGallery : ContentPage
	{
		public MultiGallery ()
		{
			var button = new Button {Text = "Toggle Nav Bar"};
			Content = new ScrollView {
				Content = new StackLayout {
					Children = {
						button,
						new Editor (),
						new Entry (),
						new Image {Source = ImageSource.FromFile ("cover1.jpg")},
						new Label {Text = "Label"},
						new ProgressBar (),
						new ActivityIndicator (),
						new Switch (),
						new Stepper (),
						new Slider (),
						new ProgressBar (),
						new ActivityIndicator (),
						new Switch (),
						new Stepper (),
						new Slider ()
					}
				}
			};

			button.Clicked +=
				(sender, args) => NavigationPage.SetHasNavigationBar (this, !NavigationPage.GetHasNavigationBar (this));

			ToolbarItems.Add (new ToolbarItem ("Back", "bank.png", () => Navigation.PopAsync (), ToolbarItemOrder.Primary));
			ToolbarItems.Add (new ToolbarItem ("It's", "bank.png", () => Navigation.PopAsync (), ToolbarItemOrder.Secondary));
			ToolbarItems.Add (new ToolbarItem ("A", "bank.png", () => Navigation.PopAsync (), ToolbarItemOrder.Secondary));
			ToolbarItems.Add (new ToolbarItem ("TARP!", "bank.png", () => Navigation.PopAsync (), ToolbarItemOrder.Secondary));
		}

		protected override void OnAppearing ()
		{
			Debug.WriteLine ("Appearing");
			base.OnAppearing ();
		}

		protected override void OnDisappearing ()
		{
			Debug.WriteLine ("Disappearing");
			base.OnDisappearing ();
		}
	}
}
