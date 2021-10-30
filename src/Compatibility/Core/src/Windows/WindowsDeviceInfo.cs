using System;
using Windows.Graphics.Display;
using Microsoft.UI.Xaml;
using Microsoft.Maui.Controls.Internals;
using WRect = Windows.Foundation.Rect;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class WindowsDeviceInfo : DeviceInfo
	{
		//DisplayInformation _information;
		bool _isDisposed;
	//	DualScreen.IDualScreenService DualScreenService => DependencyService.Get<DualScreen.IDualScreenService>();

		public WindowsDeviceInfo()
		{
			// TODO: Screen size and DPI can change at any time
			//_information = DisplayInformation.GetForCurrentView();
			//_information.OrientationChanged += OnOrientationChanged;
			//CurrentOrientation = GetDeviceOrientation(_information.CurrentOrientation);
		}

		public override Size PixelScreenSize
		{
			get
			{
				double scaling = ScalingFactor;
				Size scaled = ScaledScreenSize;
				double width = Math.Round(scaled.Width * scaling);
				double height = Math.Round(scaled.Height * scaling);

				return new Size(width, height);
			}
		}

		public override Size ScaledScreenSize
		{
			get
			{
				WRect windowSize = Forms.MainWindow.Bounds;
				return new Size(windowSize.Width, windowSize.Height);
			}
		}

		public override double ScalingFactor
		{
			get
			{
				// TODO WINUI
				return 1;
				//return ((int)_information.ResolutionScale) / 100d;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			//if (disposing)
			//{
			//	_information.OrientationChanged -= OnOrientationChanged;
			//	_information = null;
			//}

			_isDisposed = true;

			base.Dispose(disposing);
		}

		static DeviceOrientation GetDeviceOrientation(DisplayOrientations orientations)
		{

			switch (orientations)
			{
				case DisplayOrientations.Landscape:
				case DisplayOrientations.LandscapeFlipped:
					return DeviceOrientation.Landscape;

				case DisplayOrientations.Portrait:
				case DisplayOrientations.PortraitFlipped:
					return DeviceOrientation.Portrait;

				default:
				case DisplayOrientations.None:
					return DeviceOrientation.Other;
			}
		}

		void OnOrientationChanged(DisplayInformation sender, object args)
		{
			//if (DualScreenService?.IsSpanned == true)
			//{
			//	CurrentOrientation = (DualScreenService.IsLandscape) ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
			//}
			//else
			//{
			//	CurrentOrientation = GetDeviceOrientation(sender.CurrentOrientation);
			//}
		}
	}
}