using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18976 : ContentPage
{
	public Maui18976() => InitializeComponent();

	public class Test
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void DataTriggerRestoreValue(XamlInflator inflator)
		{
			var page = new Maui18976(inflator);
			Assert.False(page.checkbox.IsChecked);
			Assert.True(page.button.IsEnabled);

			page.checkbox.IsChecked = true;
			Assert.True(page.checkbox.IsChecked);
			Assert.False(page.button.IsEnabled);

			page.checkbox.IsChecked = false;
			Assert.False(page.checkbox.IsChecked);
			Assert.True(page.button.IsEnabled);
		}
	}
}
