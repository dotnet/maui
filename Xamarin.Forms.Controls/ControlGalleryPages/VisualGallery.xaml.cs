using System;

namespace Xamarin.Forms.Controls
{
	public partial class VisualGallery : ContentPage
	{
		bool isVisible = false;
		double percentage = 0.0;

		public VisualGallery()
		{
			InitializeComponent();

			BindingContext = this;
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
