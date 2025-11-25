using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18976 : ContentPage
{
	public Maui18976() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void DataTriggerRestoreValue([Values] XamlInflator inflator)
		{
			var page = new Maui18976(inflator);
			Assert.That(page.checkbox.IsChecked, Is.False);
			Assert.That(page.button.IsEnabled, Is.True);

			page.checkbox.IsChecked = true;
			Assert.That(page.checkbox.IsChecked, Is.True);
			Assert.That(page.button.IsEnabled, Is.False);

			page.checkbox.IsChecked = false;
			Assert.That(page.checkbox.IsChecked, Is.False);
			Assert.That(page.button.IsEnabled, Is.True);
		}
	}
}