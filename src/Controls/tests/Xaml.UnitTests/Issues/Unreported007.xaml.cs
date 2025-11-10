using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;
using Constraint = Microsoft.Maui.Controls.Compatibility.Constraint;
using RelativeLayout = Microsoft.Maui.Controls.Compatibility.RelativeLayout;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported007 : ContentPage
{
	public Unreported007() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests() => DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[Values]
		public void ConstraintsAreEvaluatedWithOnPlatform(XamlInflator inflator)
		{
			var page = new Unreported007(inflator);
			Assert.IsType<Constraint>(RelativeLayout.GetXConstraint(page.label));
			Assert.Equal(3, RelativeLayout.GetXConstraint(page.label).Compute(null));
		}
	}
}
