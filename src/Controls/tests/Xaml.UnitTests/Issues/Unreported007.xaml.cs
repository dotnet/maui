using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	using Constraint = Microsoft.Maui.Controls.Compatibility.Constraint;
	using RelativeLayout = Microsoft.Maui.Controls.Compatibility.RelativeLayout;

	public partial class Unreported007 : ContentPage
	{
		public Unreported007()
		{
			InitializeComponent();
		}
		public Unreported007(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[SetUp]
			public void Setup()
			{
				DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			}			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[TestCase(true), TestCase(false)]
			public void ConstraintsAreEvaluatedWithOnPlatform(bool useCompiledXaml)
			{
				var page = new Unreported007(useCompiledXaml);
				Assert.That(RelativeLayout.GetXConstraint(page.label), Is.TypeOf<Constraint>());
				Assert.Equal(3, RelativeLayout.GetXConstraint(page.label).Compute(null));
			}
		}
	}
}
