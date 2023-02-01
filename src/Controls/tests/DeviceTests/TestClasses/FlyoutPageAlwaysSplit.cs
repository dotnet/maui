using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Microsoft.Maui.DeviceTests
{
	public class FlyoutPageAlwaysSplit : FlyoutPage, IFlyoutPageController, IFlyoutView
	{
		const int DefaultFlyoutSize = 320;
		const int DefaultSmallFlyoutSize = 240;

		public FlyoutPageAlwaysSplit()
		{
			this.FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;
		}

		bool IFlyoutPageController.ShouldShowSplitMode
		{
			get
			{
				return true;
			}
		}

		double IFlyoutView.FlyoutWidth
		{
			get
			{
				if (DeviceInfo.Idiom == DeviceIdiom.Phone)
					return 50;

				var scaledScreenSize = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
				double w = scaledScreenSize.Width;
				return w < DefaultSmallFlyoutSize ? w : (w < DefaultFlyoutSize ? DefaultSmallFlyoutSize : DefaultFlyoutSize);
			}
		}
	}
}
