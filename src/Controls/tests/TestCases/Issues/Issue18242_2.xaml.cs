using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.ManualTest, "18242_2", "Button ImageSource not Scaling as expected - manual test", PlatformAffected.iOS)]
	public partial class Issue18242_2 : ContentPage
	{
		readonly Random _random;

		public Issue18242_2()
		{
			InitializeComponent();

			_random = new Random();
		}

		void OnSliderPaddingValueChanged(object sender, ValueChangedEventArgs e)
		{
			TopButton.Padding = 
			BottonButton.Padding = 
			LeftButton.Padding = 
			RightButton.Padding = new Thickness(e.NewValue);
		}

		void OnUpdateSizeButtonClicked(object sender, EventArgs e)
		{
			TopButton.HeightRequest = TopButton.WidthRequest =
			BottonButton.HeightRequest = BottonButton.WidthRequest =
			LeftButton.HeightRequest = LeftButton.WidthRequest =
			RightButton.HeightRequest = RightButton.WidthRequest =
			_random.Next(200, 300);
		}
	}
}