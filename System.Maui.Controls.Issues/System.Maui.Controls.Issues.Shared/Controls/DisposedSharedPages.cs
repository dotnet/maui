using System;

using Xamarin.Forms;

namespace Xamarin.Forms.Controls
{
	public class DisposePage : ContentPage
	{
		public event EventHandler RendererDisposed;

		public void SendRendererDisposed ()
		{
			var handler = RendererDisposed;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}

		public int DisposedLabelCount { get; private set; }

		public Action PopAction { get; set; }
		public DisposePage ()
		{
			var popButton = new Button {Text = "Pop"};
			popButton.Clicked += (sender, args) => PopAction ();

			var disposeLabel1 = new DisposeLabel {Text = "Label 1"};
			var disposeLabel2 = new DisposeLabel {Text = "Label 2"};
			var disposeLabel3 = new DisposeLabel {Text = "Label 3"};
			var disposeLabel4 = new DisposeLabel {Text = "Label 4"};
			var disposeLabel5 = new DisposeLabel {Text = "Label 5"};

			EventHandler disposeHandler = (sender, args) => DisposedLabelCount++;
			disposeLabel1.RendererDisposed += disposeHandler;
			disposeLabel2.RendererDisposed += disposeHandler;
			disposeLabel3.RendererDisposed += disposeHandler;
			disposeLabel4.RendererDisposed += disposeHandler;
			disposeLabel5.RendererDisposed += disposeHandler;

			Content = new StackLayout {
				Children = {
					popButton,
					disposeLabel1,
					disposeLabel2,
					disposeLabel3,
					disposeLabel4,
					new StackLayout {
						Children = {
							disposeLabel5,
						}
					}
				}
			};
		}
	}

	public class DisposeLabel : Label
	{
		public event EventHandler RendererDisposed;

		public void SendRendererDisposed ()
		{
			var handler = RendererDisposed;
			if (handler != null)
				handler (this, EventArgs.Empty);
		}
	}

}


