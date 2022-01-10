using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class HorizontalSwipeThresholdGallery
	{
		public HorizontalSwipeThresholdGallery()
		{
			InitializeComponent();
		}

		void OnThresholdRevealSliderChanged(object sender, ValueChangedEventArgs args)
		{
			RevealThresholdSwipeView.Close();
		}

		void OnThresholdExecuteSliderChanged(object sender, ValueChangedEventArgs args)
		{
			ExecuteThresholdSwipeView.Close();
		}
	}
}