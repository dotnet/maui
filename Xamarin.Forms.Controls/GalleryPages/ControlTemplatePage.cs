using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages
{
	internal class ControlTemplatePage : ContentPage
	{
		[Preserve (AllMembers = true)]
		class MyLayout : StackLayout
		{
			public MyLayout ()
			{
				Children.Add (new Label {Text = "Before"});
				Children.Add (new ContentPresenter ());
				Children.Add (new Label {Text = "After"});
			}
		}

		[Preserve (AllMembers = true)]
		class MyOtherLayout : StackLayout
		{
			public MyOtherLayout ()
			{
				Children.Add (new Entry {Text = "Before"});
				Children.Add (new ContentPresenter ());
				Children.Add (new Entry {Text = "After"});
			}
		}

		public ControlTemplatePage ()
		{
			var button = new Button { Text = "Replace Template" };
			var content = new ContentView {
				Content = button,
				ControlTemplate = new ControlTemplate (typeof (MyLayout))
			};

			button.Clicked += (sender, args) => {
				content.ControlTemplate = new ControlTemplate (typeof (MyOtherLayout));
			};

			Content = content;
		}
	}
}
