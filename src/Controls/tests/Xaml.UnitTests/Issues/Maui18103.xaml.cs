using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18103 : ContentPage
{
	public Maui18103() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void VSMOverride([Values] XamlInflator inflator)
		{
			var page = new Maui18103(inflator);
			Assert.That(page.button.Background, Is.EqualTo(new SolidColorBrush(Colors.Orange)));
		}
	}
}