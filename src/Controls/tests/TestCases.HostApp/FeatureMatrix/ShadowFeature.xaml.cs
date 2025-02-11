using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{
	public partial class ShadowFeature : ContentPage
	{
		bool _clip;
		bool _shadow;

		public ShadowFeature()
		{
			InitializeComponent();

			_shadow = true;

			ViewModel = new ShadowViewModel();

			BindingContext = ViewModel;
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
			if (double.TryParse(RadiusEntry.Text, out double offsetX))
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
					Center = new Point(50, 50),
					RadiusX = 25,
					RadiusY = 25
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
	}
}