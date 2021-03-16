using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages
{
	[Preserve(AllMembers = true)]
	public class GifGallery : ContentPage
	{
		public GifGallery()
		{
			var scroll = new ScrollView();

			var layout = new StackLayout
			{
				Padding = new Thickness(12)
			};

			var sourceLabel = new Label
			{
				FontSize = 10,
				Text = "Source:"
			};

			var itemsSource = new List<string>
			{
				"GifOne.gif",
				"GifTwo.gif",
				"https://devblogs.microsoft.com/wp-content/uploads/sites/44/2019/03/imagebutton-1.gif",
				"https://upload.wikimedia.org/wikipedia/commons/1/13/Rotating_earth_%28huge%29.gif" // (Huge gif file)
			};

			var sourcePicker = new Picker
			{
				ItemsSource = itemsSource,
				SelectedItem = itemsSource[0]
			};

			var IsAnimationPlayingLabel = new Label
			{
				FontSize = 10,
				Text = "IsAnimationPlaying:",
				VerticalOptions = LayoutOptions.Center
			};

			var isAnimationPlayingSwitch = new Switch
			{
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center
			};

			var gifImage = new Image
			{
				BackgroundColor = Color.LightGray,
				Source = itemsSource[0]
			};

			isAnimationPlayingSwitch.SetBinding(Switch.IsToggledProperty, nameof(gifImage.IsAnimationPlaying), BindingMode.TwoWay);
			isAnimationPlayingSwitch.BindingContext = gifImage;

			var buttonStack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var playButton = new Button
			{
				Text = "Play"
			};

			playButton.Clicked += (sender, e) =>
			{
				gifImage.IsAnimationPlaying = true;
			};

			var stopButton = new Button
			{
				Text = "Stop"
			};

			stopButton.Clicked += (sender, e) =>
			{
				gifImage.IsAnimationPlaying = false;
			};

			buttonStack.Children.Add(playButton);
			buttonStack.Children.Add(stopButton);

			layout.Children.Add(sourceLabel);
			layout.Children.Add(sourcePicker);
			layout.Children.Add(IsAnimationPlayingLabel);
			layout.Children.Add(isAnimationPlayingSwitch);
			layout.Children.Add(gifImage);
			layout.Children.Add(buttonStack);

			sourcePicker.SelectedIndexChanged += (sender, e) =>
			{
				gifImage.Source = itemsSource[sourcePicker.SelectedIndex];
			};

			scroll.Content = layout;

			Content = scroll;
		}
	}
}