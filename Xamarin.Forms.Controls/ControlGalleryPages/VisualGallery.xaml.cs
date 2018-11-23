using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public partial class VisualGallery : ContentPage
	{
		bool isVisible = false;
		public VisualGallery()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			isVisible = true;
			base.OnAppearing();
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				var progress = progressBar.Progress + 0.1;
				if (progress > 1)
					progress = 0;

				progressBar.Progress = progress;
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
