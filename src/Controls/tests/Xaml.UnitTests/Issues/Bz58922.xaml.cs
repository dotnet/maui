using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz58922 : ContentPage
	{
		public Bz58922()
		{
			InitializeComponent();
		}

		public Bz58922(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[InlineData(true)]
			[InlineData(false)]
			public void OnIdiomXDouble(bool useCompiledXaml)
			{
				mockDeviceInfo.Idiom = DeviceIdiom.Phone;
				var layout = new Bz58922(useCompiledXaml);
				Assert.Equal(320, layout.grid.HeightRequest);

				mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
				layout = new Bz58922(useCompiledXaml);
				Assert.Equal(480, layout.grid.HeightRequest);
			}
		}
	}
}