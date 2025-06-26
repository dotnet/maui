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
		}
		class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			[SetUp]
			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[Theory]
			[InlineData(true)]
			[Theory]
			[InlineData(false)]
			public void LookForInheritanceOnOpImplicit(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var p = new Bz45891(useCompiledXaml);
				Assert.Equal("Foo", p.List.First();
			}
		}
	}
}