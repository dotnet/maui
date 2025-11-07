using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22105
{
	public Maui22105() => InitializeComponent();


	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void DefaultValueShouldBeApplied(XamlInflator inflator)
		{
			var page = new Maui22105(inflator);
			Assert.Equal(100, page.label.FontSize);
		}
	}
}
