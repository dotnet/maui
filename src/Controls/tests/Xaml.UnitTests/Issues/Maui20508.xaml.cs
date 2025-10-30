using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui20508
{
	public Maui20508() => InitializeComponent();

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
		public void ToolBarItemBinding([Values] XamlInflator inflator)
		{
			var page = new Maui20508(inflator) { BindingContext = new { Icon = "boundIcon.png" } };
			Assert.That(((FileImageSource)page.ToolbarItems[0].IconImageSource).File, Is.EqualTo("boundIcon.png"));
		}
	}
}
