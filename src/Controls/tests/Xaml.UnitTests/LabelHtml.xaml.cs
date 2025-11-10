using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class LabelHtml : ContentPage
{
	public LabelHtml() => InitializeComponent();


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
		public void HtmlInCDATA(XamlInflator inflator)
		{
			var html = "<h1>Hello World!</h1><br/>SecondLine";
			var layout = new LabelHtml(inflator);
			Assert.Equal(html, layout.label0.Text);
			Assert.Equal(html, layout.label1.Text);
			Assert.Equal(html, layout.label2.Text);
			Assert.Equal(html, layout.label3.Text);
			Assert.Equal(html, layout.label4.Text);
		}
	}
}