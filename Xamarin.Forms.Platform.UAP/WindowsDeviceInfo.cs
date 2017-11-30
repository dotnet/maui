using System;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	internal class WindowsDeviceInfo : DeviceInfo
	{
		DisplayInformation _information;
		bool _isDisposed;

		public WindowsDeviceInfo()
		{
			// TODO: Screen size and DPI can change at any time
			_information = DisplayInformation.GetForCurrentView();
			_information.OrientationChanged += OnOrientationChanged;
			CurrentOrientation = GetDeviceOrientation(_information.CurrentOrientation);
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
				Rect windowSize = Window.Current.Bounds;
				return new Size(windowSize.Width, windowSize.Height);
			}
		}

		public override double ScalingFactor
		{
			get
			{
				ResolutionScale scale = _information.ResolutionScale;
				switch (scale)
				{
					case ResolutionScale.Scale120Percent:
						return 1.2;
					case ResolutionScale.Scale140Percent:
						return 1.4;
					case ResolutionScale.Scale150Percent:
						return 1.5;
					case ResolutionScale.Scale160Percent:
						return 1.6;
					case ResolutionScale.Scale180Percent:
						return 1.8;
					case ResolutionScale.Scale225Percent:
						return 2.25;
					case ResolutionScale.Scale100Percent:
					default:
						return 1;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				_information.OrientationChanged -= OnOrientationChanged;
				_information = null;
			}

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
			CurrentOrientation = GetDeviceOrientation(sender.CurrentOrientation);
		}
	}
}