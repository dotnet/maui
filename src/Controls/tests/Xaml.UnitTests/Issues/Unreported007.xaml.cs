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
		}
		public class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
			// [SetUp]
			[Xunit.Fact]
			public void Setup()
			{
				DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
			// [TearDown]
			[Xunit.Fact]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[Theory]

			[InlineData(true), InlineData(false)]
			public void ConstraintsAreEvaluatedWithOnPlatform(bool useCompiledXaml)
			{
				var page = new Unreported007(useCompiledXaml);
				Assert.IsType<Constraint>(RelativeLayout.GetXConstraint(page.label));
				Assert.Equal(3, RelativeLayout.GetXConstraint(page.label).Compute(null));
			}
		}
	}
}
