using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17597 : ContentPage
{
	public Maui17597() => InitializeComponent();

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
		public void DataTriggerInStyle([Values] XamlInflator inflator)
		{
			var page = new Maui17597(inflator);
			Assert.That(page.Test_Entry.Text, Is.EqualTo("Remove Text To Disable Button"));
			Assert.That(page.button.IsEnabled, Is.True);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "");
			Assert.That(page.Test_Entry.Text, Is.Empty);
			Assert.That(page.Test_Entry.Text.Length, Is.EqualTo(0));
			Assert.That(page.button.IsEnabled, Is.False);

			page.Test_Entry.SetValueFromRenderer(Entry.TextProperty, "foo");
			Assert.That(page.Test_Entry.Text, Is.Not.Empty);
			Assert.That(page.button.IsEnabled, Is.True);
		}
	}
}