using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WColor = Windows.UI.Color;
using WEllipse = Microsoft.UI.Xaml.Shapes.Ellipse;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WVisualStateManager = Microsoft.UI.Xaml.VisualStateManager;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public partial class SwitchRenderer : ViewRenderer<Switch, ToggleSwitch>
	{
		const string ToggleSwitchCommonStates = "CommonStates";
		const string ToggleSwitchPointerOver = "PointerOver";
		const string ToggleSwitchKnobBounds = "SwitchKnobBounds";
		const string ToggleSwitchKnobOn = "SwitchKnobOn";
		const string ToggleSwitchFillMode = "Fill";

		object _originalOnHoverColor;
		WBrush _originalOnColorBrush;
		WBrush _originalThumbOnBrush;

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
			else if (e.PropertyName == Switch.ThumbColorProperty.PropertyName)
				UpdateThumbColor();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlLoaded(object sender, RoutedEventArgs e)
		{
			UpdateOnColor();
			UpdateThumbColor();
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

		[PortHandler]
		void UpdateOnColor()
		{
			if (Control == null)
				return;

			var grid = Control.GetFirstDescendant<WGrid>();
			if (grid == null)
				return;

			var groups = WVisualStateManager.GetVisualStateGroups(grid);
			foreach (var group in groups)
			{
				if (group.Name != ToggleSwitchCommonStates)
					continue;

				foreach (var state in group.States)
				{
					if (state.Name != ToggleSwitchPointerOver)
						continue;

					foreach (var timeline in state.Storyboard.Children.OfType<ObjectAnimationUsingKeyFrames>())
					{
						var property = Storyboard.GetTargetProperty(timeline);
						var target = Storyboard.GetTargetName(timeline);

						if (target == ToggleSwitchKnobBounds && property == ToggleSwitchFillMode)
						{
							var frame = timeline.KeyFrames.FirstOrDefault();

							if (frame != null)
							{
								if (_originalOnHoverColor == null)
								{
									if (frame.Value is WColor color)
										_originalOnHoverColor = color;

									if (frame.Value is SolidColorBrush solidColorBrush)
										_originalOnHoverColor = solidColorBrush;
								}

								if (!Element.OnColor.IsDefault())
								{
									frame.Value = new WSolidColorBrush(Element.OnColor.ToWindowsColor())
									{
										Opacity = _originalOnHoverColor is WSolidColorBrush originalOnHoverBrush ? originalOnHoverBrush.Opacity : 1
									};
								}
								else
									frame.Value = _originalOnHoverColor;
							}
							break;
						}
					}
				}
			}

			var rect = Control.GetDescendantsByName<WRectangle>(ToggleSwitchKnobBounds).FirstOrDefault();

			if (rect != null)
			{
				if (_originalOnColorBrush == null)
					_originalOnColorBrush = rect.Fill;

				if (!Element.OnColor.IsDefault())
					rect.Fill = new WSolidColorBrush(Element.OnColor.ToWindowsColor());
				else
					rect.Fill = _originalOnColorBrush;
			}
		}

		[PortHandler]
		void UpdateThumbColor()
		{
			if (Control == null)
				return;

			var grid = Control.GetFirstDescendant<WGrid>();

			if (grid == null)
				return;

			var groups = WVisualStateManager.GetVisualStateGroups(grid);

			foreach (var group in groups)
			{
				if (group.Name != ToggleSwitchCommonStates)
					continue;

				foreach (var state in group.States)
				{
					if (state.Name != ToggleSwitchPointerOver)
						continue;

					foreach (var timeline in state.Storyboard.Children.OfType<ObjectAnimationUsingKeyFrames>())
					{
						var property = Storyboard.GetTargetProperty(timeline);
						var target = Storyboard.GetTargetName(timeline);

						if ((target == ToggleSwitchKnobOn) && (property == ToggleSwitchFillMode))
						{
							var frame = timeline.KeyFrames.FirstOrDefault();

							if (frame != null)
							{
								if (_originalThumbOnBrush == null)
								{
									if (frame.Value is global::Windows.UI.Color color)
										_originalOnColorBrush = new WSolidColorBrush(color);

									if (frame.Value is WBrush brush)
										_originalThumbOnBrush = brush;
								}

								if (!Element.ThumbColor.IsDefault())
								{
									var brush = Element.ThumbColor.ToPlatform();
									brush.Opacity = _originalThumbOnBrush.Opacity;
									frame.Value = brush;
								}
								else
									frame.Value = _originalThumbOnBrush;
							}
							break;
						}
					}
				}
			}

			if (grid.FindName(ToggleSwitchKnobOn) is WEllipse thumb)
			{
				if (_originalThumbOnBrush == null)
					_originalThumbOnBrush = thumb.Fill;

				if (!Element.ThumbColor.IsDefault())
					thumb.Fill = Element.ThumbColor.ToPlatform();
				else
					thumb.Fill = _originalThumbOnBrush;
			}
		}
	}
}
