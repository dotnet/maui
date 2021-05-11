using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WColor = Windows.UI.Color;
using WEllipse = Microsoft.UI.Xaml.Shapes.Ellipse;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WVisualStateManager = Microsoft.UI.Xaml.VisualStateManager;

namespace Microsoft.Maui
{
	public static class SwitchExtensions
	{
		public const string ToggleSwitchCommonStates = "CommonStates";
		public const string ToggleSwitchPointerOver = "PointerOver";
		public const string ToggleSwitchKnobBounds = "SwitchKnobBounds";
		public const string ToggleSwitchKnobOn = "SwitchKnobOn";
		public const string ToggleSwitchFillMode = "Fill";

		public static void UpdateIsToggled(this ToggleSwitch toggleSwitch, ISwitch view)
		{
			toggleSwitch.IsOn = view.IsOn;
		}

		public static void UpdateTrackColor(this ToggleSwitch toggleSwitch, ISwitch view, object? originalOnHoverColor, WBrush? originalOnColorBrush)
		{
			if (toggleSwitch == null)
				return;

			var grid = toggleSwitch.GetFirstDescendant<WGrid>();

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
								if (originalOnHoverColor == null)
								{
									if (frame.Value is WColor color)
										originalOnHoverColor = color;

									if (frame.Value is WSolidColorBrush solidColorBrush)
										originalOnHoverColor = solidColorBrush;
								}

								if (!view.TrackColor.IsDefault())
								{
									frame.Value = new WSolidColorBrush(view.TrackColor.ToWindowsColor())
									{
										Opacity = originalOnHoverColor is WSolidColorBrush originalOnHoverBrush ? originalOnHoverBrush.Opacity : 1
									};
								}
								else
									frame.Value = originalOnHoverColor;
							}
							break;
						}
					}
				}
			}

			var rect = toggleSwitch.GetDescendantsByName<WRectangle>(ToggleSwitchKnobBounds).FirstOrDefault();

			if (rect != null)
			{
				if (originalOnColorBrush == null)
					originalOnColorBrush = rect.Fill;

				if (!view.TrackColor.IsDefault())
					rect.Fill = new WSolidColorBrush(view.TrackColor.ToWindowsColor());
				else
					rect.Fill = originalOnColorBrush;
			}
		}

		public static void UpdateThumbColor(this ToggleSwitch toggleSwitch, ISwitch view, WBrush? originalThumbOnBrush = null)
		{
			if (toggleSwitch == null)
				return;

			var grid = toggleSwitch.GetFirstDescendant<WGrid>();

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
								if (originalThumbOnBrush == null)
								{
									if (frame.Value is WColor color)
										originalThumbOnBrush = new WSolidColorBrush(color);

									if (frame.Value is WBrush brush)
										originalThumbOnBrush = brush;
								}

								if (!view.ThumbColor.IsDefault())
								{
									var brush = ColorExtensions.ToNative(view.ThumbColor);

									if (originalThumbOnBrush != null)
										brush.Opacity = originalThumbOnBrush.Opacity;

									frame.Value = brush;
								}
								else
									frame.Value = originalThumbOnBrush;
							}
							break;
						}
					}
				}
			}

			if (grid.FindName(ToggleSwitchKnobOn) is WEllipse thumb)
			{
				if (originalThumbOnBrush == null)
					originalThumbOnBrush = thumb.Fill;

				if (!view.ThumbColor.IsDefault())
					thumb.Fill = ColorExtensions.ToNative(view.ThumbColor);
				else
					thumb.Fill = originalThumbOnBrush;
			}
		}
	}
}