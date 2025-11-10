using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class XamlInflatorSwitch : ContentPage
{
	public XamlInflatorSwitch() => InitializeComponent();


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
		[Fact]
		public void TestRuntimeInflator() => XamlInflatorTestsHelpers.TestInflator(typeof(XamlInflatorSwitch), XamlInflator.Runtime, true);

		[Fact]
		public void TestInflation()
		{
			var page = new XamlInflatorSwitch();
			Assert.Equal("Welcome to .NET MAUI!", page.label.Text);
		}
	}
}