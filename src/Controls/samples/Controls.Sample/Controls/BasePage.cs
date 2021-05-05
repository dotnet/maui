using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Controls
{
	public class BasePage : ContentPage, IPage
	{

		protected override void OnAppearing()
		{
			System.Diagnostics.Debug.WriteLine($"OnAppearing: {this}");
		}

		protected override void OnDisappearing()
		{
			System.Diagnostics.Debug.WriteLine($"OnDisappearing: {this}");
		}
	}
}