using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace Xamarin.Forms.Platform.UWP
{
	public class SwitchRenderer : ViewRenderer<Switch, ToggleSwitch>
	{
		Brush _originalOnHoverColor;
		Brush _originalOnColorBrush;

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = new ToggleSwitch();
					control.Toggled += OnNativeToggled;
					control.Loaded += OnControlLoaded;
					control.ClearValue(ToggleSwitch.OnContentProperty);
					control.ClearValue(ToggleSwitch.OffContentProperty);

					SetNativeControl(control);
				}

				Control.IsOn = Element.IsToggled;

				UpdateFlowDirection();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
			{
				Control.IsOn = Element.IsToggled;
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection();
			}
			else if (e.PropertyName == Switch.OnColorProperty.PropertyName)
				UpdateOnColor();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlLoaded(object sender, RoutedEventArgs e)
		{
			UpdateOnColor();
			Control.Loaded -= OnControlLoaded;
		}

		void OnNativeToggled(object sender, RoutedEventArgs routedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.IsOn);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateOnColor()
		{
			if (Control == null)
				return;

			var grid = Control.GetChildren<Windows.UI.Xaml.Controls.Grid>().FirstOrDefault();
			var groups = Windows.UI.Xaml.VisualStateManager.GetVisualStateGroups(grid);
			foreach (var group in groups)
			{
				if (group.Name != "CommonStates")
					continue;

				foreach (var state in group.States)
				{
					if (state.Name != "PointerOver")
						continue;

					foreach (var timeline in state.Storyboard.Children.OfType<ObjectAnimationUsingKeyFrames>())
					{
						var property = Storyboard.GetTargetProperty(timeline);
						var target = Storyboard.GetTargetName(timeline);
						if (target == "SwitchKnobBounds" && property == "Fill")
						{
							var frame = timeline.KeyFrames.First();

							if (_originalOnHoverColor == null)
								_originalOnHoverColor = (Brush)frame.Value;

							if (!Element.OnColor.IsDefault)
								frame.Value = new SolidColorBrush(Element.OnColor.ToWindowsColor()) { Opacity = _originalOnHoverColor.Opacity };
							else
								frame.Value = _originalOnHoverColor;
							break;
						}
					}
				}
			}

			var rect = Control.GetDescendantsByName<Windows.UI.Xaml.Shapes.Rectangle>("SwitchKnobBounds").First();

			if (_originalOnColorBrush == null)
				_originalOnColorBrush = rect.Fill;

			if (!Element.OnColor.IsDefault)
				rect.Fill = new SolidColorBrush(Element.OnColor.ToWindowsColor());
			else
				rect.Fill = _originalOnColorBrush;
		}
	}
}
