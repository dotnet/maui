using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using AView = Android.Views.View;
using Microsoft.Maui.Handlers;
using Xunit;
using ATextAlignment = Android.Views.TextAlignment;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AndroidX.DrawerLayout.Widget;

namespace Microsoft.Maui.DeviceTests
{
	public partial class FlyoutPageTests
	{
		DrawerLayout FindPlatformFlyoutView(AView aView) =>
			aView.GetParentOfType<DrawerLayout>();
	}
}