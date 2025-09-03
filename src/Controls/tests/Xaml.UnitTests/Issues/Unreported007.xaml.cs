using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;
using Constraint = Microsoft.Maui.Controls.Compatibility.Constraint;
using RelativeLayout = Microsoft.Maui.Controls.Compatibility.RelativeLayout;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported007 : ContentPage
{
	public Unreported007() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
		public void ConstraintsAreEvaluatedWithOnPlatform([Values] XamlInflator inflator)
		{
			var page = new Unreported007(inflator);
			Assert.That(RelativeLayout.GetXConstraint(page.label), Is.TypeOf<Constraint>());
			Assert.AreEqual(3, RelativeLayout.GetXConstraint(page.label).Compute(null));
		}
	}
}
