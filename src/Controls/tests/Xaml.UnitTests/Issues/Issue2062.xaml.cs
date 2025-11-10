using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2062 : ContentPage
{
	public Issue2062() => InitializeComponent();


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
		public void LabelWithoutExplicitPropertyElement(XamlInflator inflator)
		{
			var layout = new Issue2062(inflator);
			Assert.Equal("text explicitly set to Label.Text", layout.label1.Text);
			Assert.Equal("text implicitly set to Text property of Label", layout.label2.Text);
		}
	}
}