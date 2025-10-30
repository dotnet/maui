using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui26206 : ContentPage
{
	public Maui26206()
	{
		InitializeComponent();
	}


	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void MultipleResourcesInRD([Values] XamlInflator inflator)
		{
			var page = new Maui26206(inflator);
			Assert.That(((StackBase)page.Content).Spacing, Is.EqualTo(25d));
		}
	}
}
