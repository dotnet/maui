using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StringLiterals : ContentPage
{
	public StringLiterals()
	{
		InitializeComponent();
	}


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
		public void EscapedStringsAreTreatedAsLiterals(XamlInflator inflator)
		{
			var layout = new StringLiterals(inflator);
			Assert.Equal("Foo", layout.label0.Text);
			Assert.Equal("{Foo}", layout.label1.Text);
			Assert.Equal("Foo", layout.label2.Text);
			Assert.Equal("Foo", layout.label3.Text);
		}
	}
}