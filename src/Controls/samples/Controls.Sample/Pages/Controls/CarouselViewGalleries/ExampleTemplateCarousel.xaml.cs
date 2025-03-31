using System;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class ExampleTemplateCarousel : Grid
	{
		double _initialY = -1;
		bool _delete;
		readonly double _maxYScroll = 300;
		readonly double _diffYScroll = -150;
		readonly double _minYScroll = -30;

		public ExampleTemplateCarousel()
		{
			InitializeComponent();

			var gesture = new PanGestureRecognizer();

			gesture.PanUpdated += (sender, e) =>
			{
				if (e.StatusType == GestureStatus.Started)
				{
					_initialY = Y;
				}

				if (e.StatusType == GestureStatus.Running)
				{
					if (e.TotalY < _minYScroll)
					{
						var scaledValue = 1 - (Math.Abs(e.TotalY) / _maxYScroll);
						this.ScaleToAsync(0.9);
						this.FadeToAsync(scaledValue);
						this.TranslateToAsync(X, Y + e.TotalY);
					}
					if (e.TotalY < _diffYScroll)
					{
						_delete = true;
					}
				}

				if (e.StatusType == GestureStatus.Completed || e.StatusType == GestureStatus.Canceled)
				{
					if (_delete)
					{
						this.FadeToAsync(0.1);
						this.TranslateToAsync(X, Y - 1000);
						WeakReferenceMessenger.Default.Send(this, "remove");
					}
					else
					{
						this.ScaleToAsync(1);
						this.FadeToAsync(1);
						this.TranslateToAsync(X, _initialY);
					}
				}
			};

			GestureRecognizers.Add(gesture);
		}
	}
}