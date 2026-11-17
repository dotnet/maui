using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class VerticalSwipeThresholdGallery
	{
		public VerticalSwipeThresholdGallery()
		{
			InitializeComponent();
		}

		void OnThresholdRevealSliderChanged(object? sender, ValueChangedEventArgs args)
		{
			RevealThresholdSwipeView.Close();
		}

		void OnThresholdExecuteSliderChanged(object? sender, ValueChangedEventArgs args)
		{
			ExecuteThresholdSwipeView.Close();
		}
	}
}