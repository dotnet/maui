using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSPanGestureRecognizerPage : ContentPage
	{
		public iOSPanGestureRecognizerPage()
		{
			InitializeComponent();
			BindingContext = new ListViewViewModel();
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			Microsoft.Maui.Controls.Application.Current!.On<iOS>().SetPanGestureRecognizerShouldRecognizeSimultaneously(
				!Microsoft.Maui.Controls.Application.Current!.On<iOS>().GetPanGestureRecognizerShouldRecognizeSimultaneously());
		}

		void OnPanUpdated(object sender, PanUpdatedEventArgs e)
		{
			_messageLabel.Text = $"panned x:{e.TotalX} y:{e.TotalY}";
		}
	}
}
