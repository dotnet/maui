using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

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


	public class Tests : IDisposable
	{

		public void Dispose() { }
		MockDeviceInfo mockDeviceInfo;

		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[Theory]
		[Values]
		public void LookForInheritanceOnOpImplicit(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var p = new Bz45891(inflator);
			Assert.Equal("Foo", p.List.First());
		}
	}
}