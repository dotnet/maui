using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	public partial class VisualGallery : ContentPage
	{
		bool isVisible = false;
		double percentage = 0.0;
		public VisualGallery()
		{
#if APP
			InitializeComponent();
			Device.BeginInvokeOnMainThread(OnAppearing);
			pushPage.Clicked += PushPage;
#endif
		}

		async void PushPage(object sender, EventArgs e)
		{
			await Navigation.PushAsync(new VisualGallery());
		}

		public double PercentageCounter
		{
			get { return percentage; }
			set
			{
				percentage = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Counter));
			}
		}

		public double Counter => percentage * 10;

		protected override void OnAppearing()
		{
			if (!isVisible)
				return;

			isVisible = true;

			base.OnAppearing();

			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				var progress = PercentageCounter + 0.1;
				if (progress > 1)
					progress = 0;

				PercentageCounter = progress;

				return isVisible;
			});
		}

		protected override void OnDisappearing()
		{
			isVisible = false;

			base.OnDisappearing();
		}
	}
}
