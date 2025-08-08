using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13619 : ContentPage
{
	public Maui13619() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void AppThemeBindingAndDynamicResource([Values] XamlInflator inflator)
		{
			var page = new Maui13619(inflator);
			Assert.That(page.label0.TextColor, Is.EqualTo(Colors.HotPink));
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.DarkGray));

			page.Resources["Primary"] = Colors.SlateGray;
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.SlateGray));

		}
	}
}
