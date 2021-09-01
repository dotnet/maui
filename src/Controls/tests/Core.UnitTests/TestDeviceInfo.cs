using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class TestDeviceInfo : DeviceInfo
	{
		public TestDeviceInfo()
		{
			CurrentOrientation = DeviceOrientation.Portrait;
		}
		public override Size PixelScreenSize
		{
			get { return new Size(100, 200); }
		}

		public override Size ScaledScreenSize
		{
			get { return new Size(50, 100); }
		}

		public override double ScalingFactor
		{
			get { return 2; }
		}
	}
}
