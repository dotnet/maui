using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls.GalleryPages.TwoPaneViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OpenCompactWindow : ContentPage
	{
		DualScreen.DualScreenInfo info;
		public OpenCompactWindow()
		{
			InitializeComponent();
			info = new DualScreen.DualScreenInfo(layout);
			info.PropertyChanged += Info_PropertyChanged;
		}

		private void Info_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (info.SpanningBounds.Length > 0)
			{
				var bounds = info.SpanningBounds[0];
				layout.Padding = new Thickness(bounds.Width / 2 - button.Width / 2, bounds.Height / 2 - button.Height / 2, 0, 0);
			}
			else
			{
				layout.Padding = new Thickness(0);
			}
		}

		async void Button_Clicked(object sender, EventArgs e)
		{
			ContentPage page = new ContentPage() { BackgroundColor = Color.Purple };
			var button = new Button()
			{
				Text = "Close this Window"
			};

			var layout = new StackLayout()
			{
				Children =
				{
					new Label(){ Text = "Welcome to your Compact Mode Window" }, 
					button
				},
				BackgroundColor = Color.Yellow,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill
			};


			layout.BatchCommitted += Layout_BatchCommitted;
			page.Content = new ScrollView() { Content = layout };

			var args = await DualScreen.DualScreenHelper.OpenCompactMode(page);

			button.Command = new Command(async () =>
			{
				await args.CloseAsync();
			});
		}

		void Layout_BatchCommitted(object sender, Internals.EventArg<VisualElement> e)
		{
			if (sender is StackLayout layout)
				System.Diagnostics.Debug.WriteLine($"{layout.Bounds}");
		}
	}
}
