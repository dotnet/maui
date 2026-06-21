using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2016 : ContentPage
{
	public Issue2016() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void TestSwitches(XamlInflator inflator)
		{
			var page = new Issue2016(inflator);
			Assert.False(page.a0.IsToggled);
			Assert.False(page.b0.IsToggled);
			Assert.False(page.s0.IsToggled);
			Assert.False(page.t0.IsToggled);

			page.a0.IsToggled = true;
			page.b0.IsToggled = true;

			Assert.True(page.s0.IsToggled);
			Assert.True(page.t0.IsToggled);
		}
	}
}