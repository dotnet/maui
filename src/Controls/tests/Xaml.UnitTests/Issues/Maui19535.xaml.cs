using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui19535CustomThemeDictionary : ResourceDictionary
{
}

public partial class Maui19535 : Maui19535CustomThemeDictionary
{
	public Maui19535() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void SubClassOfRDShouldNotThrow(XamlInflator inflator)
		{
			var rd = new Maui19535(inflator);
			Assert.Equal(3, rd.Count);
			Assert.True(rd.TryGetValue("CustomTheme", out var theme));
			Assert.Equal("LightTheme", theme);
		}
	}

}
