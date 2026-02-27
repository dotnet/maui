using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17597 : ContentPage
{
	public Maui17597() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void DataTriggerInStyle(XamlInflator inflator)
		{
			var page = new Maui17597(inflator);
			Assert.Equal("Remove Text To Disable Button", page.Test_Entry.Text);
			Assert.True(page.button.IsEnabled);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "");
			Assert.Empty(page.Test_Entry.Text);
			Assert.Equal(0, page.Test_Entry.Text.Length);
			Assert.False(page.button.IsEnabled);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "foo");
			Assert.NotEmpty(page.Test_Entry.Text);
			Assert.True(page.button.IsEnabled);
		}
	}
}