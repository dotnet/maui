using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls
{
	public class DisposeGallery : ContentPage
	{
		public DisposeGallery ()
		{
			var disposePage = new DisposePage ();

			var pushButton = new Button {Text = "Push disposable page"};
			var pushModalButton = new Button {Text = "PushModal disposable page"};
			
			pushButton.Clicked += (sender, args) => {
				disposePage.PopAction = () => Navigation.PopAsync ();
				Navigation.PushAsync (disposePage);
			};
			pushModalButton.Clicked += (sender, args) => {
				disposePage.PopAction = () => Navigation.PopModalAsync ();
				Navigation.PushModalAsync (disposePage);
			};

			var appearingLabel = new Label {Text = "Appearing not sent"};
			var disappearingLabel = new Label {Text = "Disappearing not sent"};

			var disposedPageLabel = new Label {Text = "Page renderer not yet disposed"};
			var disposedLabelsLabel = new Label {Text = "Number of disposed labels: 0"};

			disposePage.Appearing += (sender, args) => appearingLabel.Text = "Appearing sent";
			disposePage.Disappearing += (sender, args) => disappearingLabel.Text = "Disappearing sent";
			disposePage.RendererDisposed += (sender, args) => {
				disposedPageLabel.Text = "Page renderer disposed";
				// give some time for this to propogate
				Device.StartTimer (TimeSpan.FromSeconds (1), () => {
					disposedLabelsLabel.Text = "Number of disposed labels: " + disposePage.DisposedLabelCount;
					return false;
				});

			};

			Content = new StackLayout {
				Padding = new Thickness (20),
				Children = {
					pushButton,
					pushModalButton,
					appearingLabel,
					disappearingLabel,
					disposedLabelsLabel,
					disposedPageLabel
				}
			};
		}
	}
}
