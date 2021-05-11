#nullable enable
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WColor = Windows.UI.Color;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;
using WVisualStateManager = Microsoft.UI.Xaml.VisualStateManager;

namespace Microsoft.Maui.Handlers
{
	public partial class SwitchHandler : ViewHandler<ISwitch, ToggleSwitch>
	{
		object? _originalOnHoverColor;
		WBrush? _originalOnColorBrush;
		WBrush? _originalThumbOnBrush;

		protected override ToggleSwitch CreateNativeView() => new ToggleSwitch();

		protected override void SetupDefaults(ToggleSwitch nativeView)
		{
			var grid = nativeView.GetFirstDescendant<WGrid>();

			if (grid == null)
				return;

			var groups = WVisualStateManager.GetVisualStateGroups(grid);

			foreach (var group in groups)
			{
				if (group.Name != SwitchExtensions.ToggleSwitchCommonStates)
					continue;

				foreach (var state in group.States)
				{
					if (state.Name != SwitchExtensions.ToggleSwitchPointerOver)
						continue;

					foreach (var timeline in state.Storyboard.Children.OfType<ObjectAnimationUsingKeyFrames>())
					{
						var property = Storyboard.GetTargetProperty(timeline);
						var target = Storyboard.GetTargetName(timeline);

						if ((target == SwitchExtensions.ToggleSwitchKnobOn) && (property == SwitchExtensions.ToggleSwitchFillMode))
						{
							var frame = timeline.KeyFrames.FirstOrDefault();

							if (frame != null)
							{
								if (_originalThumbOnBrush == null)
								{
									if (frame.Value is WColor color)
										_originalThumbOnBrush = new WSolidColorBrush(color);

									if (frame.Value is WBrush brush)
										_originalThumbOnBrush = brush;
								}
							}
						}

						if (target == SwitchExtensions.ToggleSwitchKnobBounds && property == SwitchExtensions.ToggleSwitchFillMode)
						{
							var frame = timeline.KeyFrames.FirstOrDefault();

							if (frame != null)
							{
								if (_originalOnHoverColor == null)
								{
									if (frame.Value is WColor color)
										_originalOnHoverColor = color;

									if (frame.Value is WSolidColorBrush solidColorBrush)
										_originalOnHoverColor = solidColorBrush;
								}
							}
						}
					}
				}
			}

			var rect = nativeView.GetDescendantsByName<WRectangle>(SwitchExtensions.ToggleSwitchKnobBounds).FirstOrDefault();

			if (rect != null)
			{
				if (_originalOnColorBrush == null)
					_originalOnColorBrush = rect.Fill;
			}
		}

		public static void MapIsOn(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateIsToggled(view);
		}

		public static void MapTrackColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateTrackColor(view, handler._originalOnHoverColor, handler._originalOnColorBrush);
		}

		public static void MapThumbColor(SwitchHandler handler, ISwitch view)
		{
			handler.NativeView?.UpdateThumbColor(view, handler._originalThumbOnBrush);
		}

		protected override void DisconnectHandler(ToggleSwitch nativeView)
		{
			base.DisconnectHandler(nativeView);
			nativeView.Toggled -= OnToggled;
		}

		protected override void ConnectHandler(ToggleSwitch nativeView)
		{
			base.ConnectHandler(nativeView);
			nativeView.Toggled += OnToggled;
		}

		void OnToggled(object sender, UI.Xaml.RoutedEventArgs e)
		{
			if (VirtualView == null || NativeView == null)
				return;

			VirtualView.IsOn = NativeView.IsOn;
		}
	}
}