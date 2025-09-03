using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported010
{
	public Unreported010() => InitializeComponent();

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
		public void LocalDynamicResources([Values] XamlInflator inflator)
		{
			var page = new Unreported010(inflator);
			Assert.That(page.button0.BackgroundColor, Is.EqualTo(Colors.Blue));
			page.Resources["Foo"] = Colors.Red;
			Assert.That(page.button0.BackgroundColor, Is.EqualTo(Colors.Red));
		}
	}


}
