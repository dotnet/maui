using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	public partial class ShadowFeaturePage : ContentPage
	{
		const double Fps = 60;

		bool _clip;
		bool _shadow;
		bool _benchmark;

		DateTime _lastUpdateDateTime;
		readonly IDispatcherTimer _timer;

		event Action timerUpdateEvent;

		public ShadowFeaturePage()
		{
			InitializeComponent();

			_shadow = true;
			_lastUpdateDateTime = DateTime.Now;

			_timer = Dispatcher.CreateTimer();
			_timer.Interval = TimeSpan.FromSeconds(1 / Fps);
			double time = 0;
			DateTime currentTickDateTime = DateTime.Now;
			double deltaTime = 0;

			_timer.Tick += delegate
			{
				deltaTime = (DateTime.Now - currentTickDateTime).TotalSeconds;
				currentTickDateTime = DateTime.Now;
				time += deltaTime;
				timerUpdateEvent?.Invoke();
			};

			timerUpdateEvent += delegate
			{
				double sinVal = (Math.Sin(time) + 1) * 0.5;
				const double minSize = 20;
				double width = minSize + sinVal * 100;

				BorderShadow.WidthRequest = ImageShadow.WidthRequest = LabelShadow.WidthRequest = width;
			};

			ViewModel = new ShadowViewModel();

			BindingContext = ViewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			ShadowContainer.SizeChanged += OnShadowSizeChanged;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			ShadowContainer.SizeChanged -= OnShadowSizeChanged;
			_timer.Stop();
		}

		void OnShadowSizeChanged(object sender, EventArgs e)
		{
			if (_lastUpdateDateTime != DateTime.Now)
			{
				string fps = Math.Round(1 / (DateTime.Now - _lastUpdateDateTime).TotalSeconds, 2).ToString();
				FpsLabel.Text = fps;

				_lastUpdateDateTime = DateTime.Now;
			}
		}

		public ShadowViewModel ViewModel { get; private set; }

		void OnColorChanged(object sender, TextChangedEventArgs e)
		{
			Color.TryParse(ColorEntry.Text, out Color color);

			if (color is not null)
				ViewModel.Color = color;
		}

		void OnOffsetXChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(OffsetXEntry.Text, out double offsetX))
			{
				ViewModel.OffsetX = offsetX;
			}
		}

		void OnOffsetYChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(OffsetYEntry.Text, out double offsetY))
			{
				ViewModel.OffsetY = offsetY;
			}
		}

		void OnRadiusChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(RadiusEntry.Text, out double radius))
			{
				ViewModel.Radius = radius;
			}
		}

		void OnOpacityChanged(object sender, TextChangedEventArgs e)
		{
			if (double.TryParse(OpacityEntry.Text, out double opacity))
			{
				ViewModel.Opacity = opacity;
			}
		}

		void OnFlowDirectionChanged(object sender, EventArgs e)
		{
			ViewModel.FlowDirection = FlowDirectionLTR.IsChecked ? FlowDirection.LeftToRight : FlowDirection.RightToLeft;
		}

		void OnIsEnabledCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			ViewModel.IsEnabled = IsEnabledTrueRadio.IsChecked;
		}

		void OnIsVisibleCheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			ViewModel.IsVisible = IsVisibleTrueRadio.IsChecked;
		}

		void OnBenchmarkClicked(object sender, EventArgs e)
		{
			if (_benchmark)
			{
				FpsLabel.IsVisible = false;
				_timer.Stop();
				_benchmark = false;
			}
			else
			{
				FpsLabel.IsVisible = true;
				_timer.Start();
				_benchmark = true;
			}
		}

		void OnClipClicked(object sender, EventArgs e)
		{
			if (_clip)
			{
				ClipButton.Text = "Add Clip";
				BorderShadow.Clip = ImageShadow.Clip = LabelShadow.Clip = null;
				_clip = false;
			}
			else
			{
				ClipButton.Text = "Remove Clip";
				BorderShadow.Clip = ImageShadow.Clip = LabelShadow.Clip = new EllipseGeometry
				{
					Center = new Point(40, 40),
					RadiusX = 20,
					RadiusY = 20
				};
				_clip = true;
			}
		}

		void OnShadowClicked(object sender, EventArgs e)
		{
			if (_shadow)
			{
				ShadowButton.Text = "Add Shadow";
				BorderShadow.Shadow = ImageShadow.Shadow = LabelShadow.Shadow = null;
				_shadow = false;
			}
			else
			{
				ShadowButton.Text = "Remove Shadow";

				var newShadow = new Shadow();

				newShadow.SetBinding(Shadow.BrushProperty, "Color");
				newShadow.SetBinding(Shadow.OffsetProperty, "Offset");
				newShadow.SetBinding(Shadow.RadiusProperty, "Radius");
				newShadow.SetBinding(Shadow.OpacityProperty, "Opacity");

				BorderShadow.Shadow = ImageShadow.Shadow = LabelShadow.Shadow = newShadow;
				_shadow = true;
			}
		}

		void OnResetClicked(object sender, EventArgs e)
		{
			FpsLabel.IsVisible = false;
			_timer.Stop();
			_benchmark = false;

			ColorEntry.Text = "#000000";

			IsEnabledTrueRadio.IsChecked = true;
			IsVisibleTrueRadio.IsChecked = true;
			FlowDirectionLTR.IsChecked = true;

			BorderShadow.Clip = ImageShadow.Clip = LabelShadow.Clip = null;
			BorderShadow.WidthRequest = ImageShadow.WidthRequest = LabelShadow.WidthRequest = 80;
		}
	}
}