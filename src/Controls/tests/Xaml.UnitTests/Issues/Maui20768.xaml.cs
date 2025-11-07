using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20768
{
	public Maui20768() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void BindingsDoNotResolveStaticProperties(XamlInflator inflator)
		{
			var page = new Maui20768(inflator);
			page.TitleLabel.BindingContext = new ViewModel20768();
			Assert.Null(page.TitleLabel.Text);
		}
	}
}

public class ViewModel20768
{
	public static string Title => "Title from static property";
}
