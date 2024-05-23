using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class HybridWebViewPage
	{
		public HybridWebViewPage()
		{
			InitializeComponent();
		}

		private void SendMessageButton_Clicked(object sender, EventArgs e)
		{
			hwv.SendRawMessage("Hello from C#!");
		}

		private void hwv_RawMessageReceived(object sender, HybridWebViewRawMessageReceivedEventArgs e)
		{
			Dispatcher.Dispatch(() => statusLabel.Text += e.Message);
		}
	}
}
