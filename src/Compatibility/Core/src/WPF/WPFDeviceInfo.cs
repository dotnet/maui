using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	internal class WPFDeviceInfo : DeviceInfo
	{
		internal const string BWPorientationChangedName = "Xamarin.WPF.OrientationChanged";
#pragma warning disable 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
		readonly double _scalingFactor;
#pragma warning restore 0649

		public WPFDeviceInfo()
		{
			MessagingCenter.Subscribe(this, BWPorientationChangedName, (FormsApplicationPage page, DeviceOrientation orientation) => { CurrentOrientation = orientation; });

			var content = System.Windows.Application.Current.MainWindow;

			// Scaling Factor for Windows Phone 8 is relative to WVGA: https://msdn.microsoft.com/en-us/library/windows/apps/jj206974(v=vs.105).aspx
			//_scalingFactor = content.ScaleFactor / 100d;
			//PixelScreenSize = new Size(content.ActualWidth * _scalingFactor, content.ActualHeight * _scalingFactor);
			PixelScreenSize = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
			ScaledScreenSize = new Size(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
		}

		public override Size PixelScreenSize { get; }

		public override Size ScaledScreenSize { get; }

		public override double ScalingFactor
		{
			get { return _scalingFactor; }
		}

		protected override void Dispose(bool disposing)
		{
			MessagingCenter.Unsubscribe<FormsApplicationPage, DeviceOrientation>(this, BWPorientationChangedName);
			base.Dispose(disposing);
		}
	}
}
