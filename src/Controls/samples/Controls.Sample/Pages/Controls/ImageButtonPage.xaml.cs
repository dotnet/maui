using System;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ImageButtonPage
	{
		int _clickTotal;

		public ImageButtonPage()
		{
			InitializeComponent();

			BindingContext = new ImageButtonPageViewModel();

			ImageButton01.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == ImageButton.IsLoadingProperty.PropertyName)
					Debug.WriteLine($"{e.PropertyName}: {ImageButton01.IsLoading}");
			};

			UpdateImageButtonBackground();
		}

		void OnImageButtonClicked(object sender, EventArgs e)
		{
			_clickTotal += 1;
			InfoLabel.Text = $"{_clickTotal} ImageButton click{(_clickTotal == 1 ? "" : "s")}";
		}

		void OnResizeImageButtonClicked(object sender, EventArgs e)
		{
			ResizeImageButton.HeightRequest = 100;
			ResizeImageButton.WidthRequest = 100;
		}

		void UseOnlineSource_Clicked(object sender, EventArgs e)
		{
			AnimatedGifImage.Source =
				ImageSource.FromUri(new Uri("https://raw.githubusercontent.com/dotnet/maui/126f47aaf9d5c01224f54fe1c6bfb1c8299cc2fe/src/Compatibility/ControlGallery/src/iOS/GifTwo.gif"));
		}

		void OnUpdateBackgroundButtonClicked(object sender, EventArgs e)
		{
			UpdateImageButtonBackground();
		}

		void OnRemoveBackgroundButtonClicked(object sender, EventArgs e)
		{
			BackgroundImageButton.Background = null;
		}

		void UpdateImageButtonBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundImageButton.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = startColor },
					new GradientStop { Color = endColor, Offset = 1 }
				}
			};
		}
	}

	public class ImageButtonPageViewModel : BindableObject
	{
		public ICommand ImageButtonCommand => new Command(OnExecuteImageButtonCommand);

		void OnExecuteImageButtonCommand()
		{
			Debug.WriteLine("Command");
		}
	}
}