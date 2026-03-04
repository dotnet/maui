using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public class SliderControlPage : NavigationPage
	{
		private SliderViewModel _viewModel;

		public SliderControlPage()
		{
			_viewModel = new SliderViewModel();
#if ANDROID
			BarTextColor = Colors.White;
#endif
			PushAsync(new SliderControlMainPage(_viewModel));
		}
	}

	public partial class SliderControlMainPage : ContentPage
	{
		private SliderViewModel _viewModel;

		public SliderControlMainPage(SliderViewModel viewModel)
		{
			InitializeComponent();
			_viewModel = viewModel;
			BindingContext = _viewModel;
		}

		private async void NavigateToOptionsPage_Clicked(object sender, EventArgs e)
		{
			BindingContext = _viewModel = new SliderViewModel();
			ReInitializeSlider();
			await Navigation.PushAsync(new SliderOptionsPage(_viewModel));
		}

		private void ReInitializeSlider()
		{
			SliderGrid.Children.Clear();

			var slider = new Slider
			{
				Margin = new Thickness(0, 100, 0, 100),
				AutomationId = "SliderControl"
			};

			// Set up bindings
			slider.SetBinding(Slider.MinimumProperty, new Binding("Minimum"));
			slider.SetBinding(Slider.MaximumProperty, new Binding("Maximum"));
			slider.SetBinding(Slider.ValueProperty, new Binding("Value"));
			slider.SetBinding(Slider.ThumbColorProperty, new Binding("ThumbColor"));
			slider.SetBinding(Slider.MinimumTrackColorProperty, new Binding("MinTrackColor"));
			slider.SetBinding(Slider.MaximumTrackColorProperty, new Binding("MaxTrackColor"));
			slider.SetBinding(Slider.BackgroundColorProperty, new Binding("BackgroundColor"));
			slider.SetBinding(Slider.IsVisibleProperty, new Binding("IsVisible"));
			slider.SetBinding(Slider.IsEnabledProperty, new Binding("IsEnabled"));
			slider.SetBinding(Slider.FlowDirectionProperty, new Binding("FlowDirection"));
			slider.SetBinding(Slider.ThumbImageSourceProperty, new Binding("ThumbImageSource"));
			slider.SetBinding(Slider.DragStartedCommandProperty, new Binding("DragStartedCommand"));
			slider.SetBinding(Slider.DragCompletedCommandProperty, new Binding("DragCompletedCommand"));

			SliderGrid.Children.Add(slider);
		}
	}
}
