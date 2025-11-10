using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class TestXmlnsUsing : ContentPage
{
	public TestXmlnsUsing() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void SupportUsingXmlns(XamlInflator inflator)
		{
			var page = new TestXmlnsUsing(inflator);
			Assert.NotNull(page.Content);
			Assert.IsType<CustomXamlView>(page.CustomView);
			Assert.Equal(1, page.Radio1.Value);
			Assert.Equal(2, page.Radio2.Value);
		}
	}
}
