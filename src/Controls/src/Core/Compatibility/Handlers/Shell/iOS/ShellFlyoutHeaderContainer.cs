using System;
using Microsoft.Maui.Platform;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	internal class ShellFlyoutHeaderContainer : UIContainerView
	{
		public ShellFlyoutHeaderContainer(View view) : base(view)
		{
		}

		public override Thickness Margin
		{
			get
			{
				if (!View.IsSet(View.MarginProperty))
				{
					var newMargin = new Thickness(0, (float)UIApplication.SharedApplication.GetSafeAreaInsetsForWindow().Top, 0, 0);

					if (newMargin != View.Margin)
					{
						View.Margin = newMargin;
					}
				}

				return View.Margin;
			}
		}
	}
}
