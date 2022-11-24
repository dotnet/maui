#nullable enable

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WGrid = Microsoft.UI.Xaml.Controls.Grid;
using WRectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace Microsoft.Maui.Controls.Platform
{
	class ShellSplitView
	{
		Brush? _flyoutBackdrop;
		WBrush? _flyoutPlatformBrush;
		WBrush? _defaultBrush;
		WRectangle? _dismissLayer;
		FrameworkElement? _paneRoot;
		WRectangle? _HCPaneBorder;

		LightDismissOverlayMode? _defaultLightDismissOverlayMode;
		double _height = -1d;
		double _width = -1d;
		//double _defaultOpenPaneLength = -1d;
		SplitView _splitView;

		public ShellSplitView(SplitView splitView)
		{
			_splitView = splitView;
		}

		internal void SetFlyoutSizes(double height, double width)
		{
			_height = height;
			_width = width;
		}

		internal void RefreshFlyoutPosition()
		{
			_paneRoot ??= _splitView.GetDescendantByName<FrameworkElement>("PaneRoot");
			if (_paneRoot == null)
				return;

			_HCPaneBorder ??= _splitView.GetDescendantByName<WRectangle>("HCPaneBorder");

			if (_paneRoot != null)
			{
				if (_height == -1)
				{
					_paneRoot.Height = double.NaN;
					_paneRoot.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;

					if (_HCPaneBorder != null)
						_HCPaneBorder.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
				}
				else
				{
					_paneRoot.Height = _height;
					_paneRoot.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Top;

					if (_HCPaneBorder != null)
						_HCPaneBorder.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
				}
			}
		}

		internal void RefreshFlyoutBackdrop()
		{
			// Because shell is currently Nesting Navigation Views we have to be careful and make sure to retrive the correct one
			// If we just do a straight search for "LightDismissLayer it will return the wrong one.
			if (_dismissLayer == null)
			{
				var contentRoot = _splitView.GetDescendantByName<WGrid>("ContentRoot");
				if (contentRoot != null)
				{
					foreach (var child in contentRoot.Children)
					{
						if (child is WRectangle maybe &&
							$"{child.GetValue(FrameworkElement.NameProperty)}" == "LightDismissLayer")
						{
							_dismissLayer = maybe;
							break;
						}
					}
				}
			}

			if (_dismissLayer == null)
				return;

			if (_defaultBrush == null)
				_defaultBrush = _dismissLayer.Fill;

			if (Brush.IsNullOrEmpty(_flyoutBackdrop))
			{
				_dismissLayer.Fill = _defaultBrush;
			}
			else
			{
				_dismissLayer.Fill = _flyoutPlatformBrush ?? _defaultBrush;
			}
		}

		internal Brush FlyoutBackdrop
		{
			set
			{
				if (_flyoutBackdrop == value)
					return;

				_flyoutBackdrop = value;

				if (_defaultLightDismissOverlayMode == null)
					_defaultLightDismissOverlayMode = _splitView.LightDismissOverlayMode;

				if (value == Brush.Default)
				{
					_splitView.LightDismissOverlayMode = _defaultLightDismissOverlayMode ?? LightDismissOverlayMode.Auto;
				}
				else
				{
					_splitView.LightDismissOverlayMode = LightDismissOverlayMode.On;
				}

				if (_flyoutBackdrop != null)
					_flyoutPlatformBrush = _flyoutBackdrop.ToBrush();
				else
					_flyoutPlatformBrush = _defaultBrush;
			}
		}
	}
}
