using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz45891 : ContentPage
	{
		public Bz45891()
		{
			InitializeComponent();
		}

		public Bz45891(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		public static readonly BindableProperty ListProperty =
			BindableProperty.Create("List", typeof(IEnumerable<string>), typeof(Bz45891), default(IEnumerable<string>));

		public IEnumerable<string> List
		{
			get { return (IEnumerable<string>)GetValue(ListProperty); }
			set { SetValue(ListProperty, value); }
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
			public void LookForInheritanceOnOpImplicit(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var p = new Bz45891(useCompiledXaml);
				Assert.Equal("Foo", p.List.First());
			}
		}
	}
}