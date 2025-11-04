using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz45891 : ContentPage
{
	public Bz45891() => InitializeComponent();

	public static readonly BindableProperty ListProperty =
		BindableProperty.Create(nameof(List), typeof(IEnumerable<string>), typeof(Bz45891), default(IEnumerable<string>));

	public IEnumerable<string> List
	{
		get { return (IEnumerable<string>)GetValue(ListProperty); }
		set { SetValue(ListProperty, value); }
	}

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp] public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
		public void LookForInheritanceOnOpImplicit([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var p = new Bz45891(inflator);
			Assert.AreEqual("Foo", p.List.First());
		}
	}
}