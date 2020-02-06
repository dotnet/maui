using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls
{
	internal class MediaElementDemoPage : ContentPage
	{
		MediaElement element;
		Label positionLabel;
		Label consoleLabel;

		public MediaElementDemoPage()
		{
			element = new MediaElement();
			element.HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill,true);
			element.VerticalOptions = new LayoutOptions(LayoutAlignment.Fill,true);
			element.MinimumWidthRequest = 320;
			element.MinimumHeightRequest = 240;
			element.AutoPlay = false;
			element.Aspect = Aspect.AspectFill;
			element.ShowsPlaybackControls = true;
			element.BackgroundColor = Color.Red;
			element.MediaEnded += Element_MediaEnded;
			element.MediaFailed += Element_MediaFailed;
			element.MediaOpened += Element_MediaOpened;
			consoleLabel = new Label();

			var infoStack = new StackLayout { Orientation = StackOrientation.Horizontal };

			var stateLabel = new Label();
			stateLabel.SetBinding(Label.TextProperty, new Binding("CurrentState", BindingMode.OneWay, null, null, "s:{0}", element));
			var bufferingLabel = new Label();
			bufferingLabel.SetBinding(Label.TextProperty, new Binding("BufferingProgress", BindingMode.OneWay, null, null, "b:{0:f2}", element));
			var heightLabel = new Label();
			heightLabel.SetBinding(Label.TextProperty, new Binding("VideoHeight", BindingMode.OneWay, null, null, "h:{0}", element));
			var widthLabel = new Label();
			widthLabel.SetBinding(Label.TextProperty, new Binding("VideoWidth", BindingMode.OneWay, null, null, "w:{0}", element));
			var durationLabel = new Label();
			durationLabel.SetBinding(Label.TextProperty, new Binding("Duration", BindingMode.OneWay, null, null, "d:{0:g}", element));
			var volumeLabel = new Label();
			volumeLabel.SetBinding(Label.TextProperty, new Binding("Volume", BindingMode.OneWay, null, null, "v:{0}", element));
			infoStack.Children.Add(stateLabel);
			infoStack.Children.Add(bufferingLabel);
			infoStack.Children.Add(heightLabel);
			infoStack.Children.Add(widthLabel);
			infoStack.Children.Add(durationLabel);
			infoStack.Children.Add(volumeLabel);

			positionLabel = new Label();
			positionLabel.TextColor = Color.Black;
			//positionLabel.SetBinding(Label.TextProperty, new Binding("Position", BindingMode.OneWay, null, null, "{0:g}", element));

			var playButton = new Button();
			playButton.Text = "\u25b6\uFE0F";
			playButton.FontSize = 48;
			playButton.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, true);
			playButton.Clicked += PlayButton_Clicked;

			var pauseButton = new Button();
			pauseButton.Text = "\u23f8\uFE0F";
			pauseButton.FontSize = 48;
			pauseButton.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, true);
			pauseButton.Clicked += PauseButton_Clicked;

			var stopButton = new Button();
			stopButton.Text = "\u23f9\uFE0F";
			stopButton.FontSize = 48;
			stopButton.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, true);
			stopButton.Clicked += StopButton_Clicked;

			var showControlsSwitch = new Switch();
			showControlsSwitch.SetBinding(Switch.IsToggledProperty, new Binding("ShowsPlaybackControls", BindingMode.TwoWay, source: element));

			
			var mediaControlStack = new StackLayout();
			mediaControlStack.Orientation = StackOrientation.Horizontal;
			mediaControlStack.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false);
			mediaControlStack.Children.Add(playButton);
			mediaControlStack.Children.Add(pauseButton);
			mediaControlStack.Children.Add(stopButton);
			mediaControlStack.Children.Add(showControlsSwitch);

			var aspectFitButton = new Button() { Text = "Aspect Fit" };
			aspectFitButton.Clicked += (s, e) => { element.Aspect = Aspect.AspectFit; };
			var aspectFillButton = new Button() { Text = "Aspect Fill" };
			aspectFillButton.Clicked += (s, e) => { element.Aspect = Aspect.AspectFill; };
			var fillButton = new Button() { Text = "Fill" };
			fillButton.Clicked += (s, e) => { element.Aspect = Aspect.Fill; };

			var aspectStack = new StackLayout { Orientation = StackOrientation.Horizontal, HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false) };
			aspectStack.Children.Add(aspectFitButton);
			aspectStack.Children.Add(aspectFillButton);
			aspectStack.Children.Add(fillButton);

			var volumeSlider = new Slider()
			{
				Minimum = 0,
				Maximum = 1
			};
			volumeSlider.Value = 0.1;

			volumeSlider.ValueChanged += VolumeSlider_ValueChanged;

			var stack = new StackLayout();
			stack.Padding = new Thickness(10);
			stack.Spacing = 10;
			stack.HorizontalOptions = new LayoutOptions(LayoutAlignment.Fill, false);
			stack.VerticalOptions = new LayoutOptions(LayoutAlignment.Fill, false);
			stack.Children.Add(element);
			stack.Children.Add(infoStack);
			stack.Children.Add(positionLabel);
			stack.Children.Add(mediaControlStack);
			stack.Children.Add(aspectStack);
			stack.Children.Add(consoleLabel);
			stack.Children.Add(new Label() { Text = "Volume:" });
			stack.Children.Add(volumeSlider);
			element.Volume = 0.1;
			Content = stack;	
		}

		private void VolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
		{
			element.Volume = (sender as Slider).Value;
		}

		void Element_MediaOpened(object sender, EventArgs e)
		{
			consoleLabel.Text += "Media opened" + Environment.NewLine;
		}

		void Element_MediaFailed(object sender, EventArgs e)
		{
			consoleLabel.Text += "Media failed" + Environment.NewLine;
		}

		void Element_MediaEnded(object sender, EventArgs e)
		{
			consoleLabel.Text += "Media ended" + Environment.NewLine;
		}

		void PlayButton_Clicked(object sender, EventArgs e)
		{
			element.Play();
		}

		void PauseButton_Clicked(object sender, EventArgs e)
		{
			element.Pause();
		}

		void StopButton_Clicked(object sender, EventArgs e)
		{
			element.Stop();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if(Device.RuntimePlatform == Device.WPF)
			{
				// workaround for lack of https support on WPF
				element.Source = new Uri("http://sec.ch9.ms/ch9/5d93/a1eab4bf-3288-4faf-81c4-294402a85d93/XamarinShow_mid.mp4");
			}
			else
			{
				element.Source = new Uri("https://sec.ch9.ms/ch9/5d93/a1eab4bf-3288-4faf-81c4-294402a85d93/XamarinShow_mid.mp4");			
			}

			Device.StartTimer(TimeSpan.FromMilliseconds(100), ()=>{
				Device.BeginInvokeOnMainThread(() =>
				{
					positionLabel.Text = element.Position.ToString("mm\\:ss\\.ff");
				});

				return polling;
			});
		}

		bool polling = true;

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			polling = false;
		}
	}
}
