using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class Maui8149 : ContentView
{
	public Maui8149() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void NamescopeWithXamlC([Values] XamlInflator inflator)
		{
			var page = new Maui8149(inflator);
			Assert.That((page.Content as Maui8149View).Text, Is.EqualTo("Microsoft.Maui.Controls.Xaml.UnitTests.Maui8149"));
		}
	}
}