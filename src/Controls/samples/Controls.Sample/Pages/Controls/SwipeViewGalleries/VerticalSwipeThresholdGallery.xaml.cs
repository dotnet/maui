using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class VerticalSwipeThresholdGallery
	{
		public VerticalSwipeThresholdGallery()
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