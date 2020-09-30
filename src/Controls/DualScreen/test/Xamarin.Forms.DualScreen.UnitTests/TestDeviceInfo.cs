using Xamarin.Forms.Internals;

namespace Xamarin.Forms.DualScreen.UnitTests
{
	internal class TestDeviceInfo : DeviceInfo
	{
		public TestDeviceInfo()
		{
			CurrentOrientation = DeviceOrientation.Portrait;
		}

		public override Size PixelScreenSize
		{
			get 
			{ 
				if(CurrentOrientation == DeviceOrientation.Landscape)
					return new Size(1000, 2000); 
				else
					return new Size(2000, 1000);
			}
		}

		public override Size ScaledScreenSize
		{
			get
			{
				var pixelSize = PixelScreenSize;
				return new Size(pixelSize.Width / ScalingFactor, pixelSize.Height / ScalingFactor);
			}
		}

		public override double ScalingFactor
		{
			get { return 2; }
		}
	}

	internal class TestDeviceInfoPortrait : TestDeviceInfo
	{
		public TestDeviceInfoPortrait()
		{
			CurrentOrientation = DeviceOrientation.Portrait;
		}
	}

	internal class TestDeviceInfoLandscape : TestDeviceInfo
	{
		public TestDeviceInfoLandscape()
		{
			CurrentOrientation = DeviceOrientation.Landscape;
		}
	}
}
