using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries
{
	public partial class ExampleTemplateCarousel
	{
		double initialY = -1;
		bool delete;
		double maxYScroll = 300;
		double diffYScroll = -150;
		double minYScroll = -30;

		public ExampleTemplateCarousel()
		{
			InitializeComponent();

			var gesture = new PanGestureRecognizer();

			gesture.PanUpdated += (sender, e) =>
			{
				if (e.StatusType == GestureStatus.Started)
				{
					initialY = Y;
				}

				if (e.StatusType == GestureStatus.Running)
				{
					if (e.TotalY < minYScroll)
					{
						var scaledValue = 1 - (Math.Abs(e.TotalY) / maxYScroll);
						this.ScaleTo(0.9);
						this.FadeTo(scaledValue);
						this.TranslateTo(X, Y + e.TotalY);
					}
					if (e.TotalY < diffYScroll)
					{
						delete = true;
					}
				}

				if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
				{
					if (delete)
					{
						this.FadeTo(0.1);
						this.TranslateTo(X, Y - 1000);
						MessagingCenter.Send<ExampleTemplateCarousel>(this, "remove");
					}
					else
					{
						this.ScaleTo(1);
						this.FadeTo(1);
						this.TranslateTo(X, initialY);
					}
				}
			};
			GestureRecognizers.Add(gesture);
		}
	}
}
