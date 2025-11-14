using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11857 : ContentPage
{
	public Maui11857() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void SolidColorBrushAsCompiledResources([Values] XamlInflator inflator)
		{
			//shouldn't throw
			var page = new Maui11857(inflator);
			Assert.AreEqual(Colors.HotPink, ((SolidColorBrush)page.label.Background).Color);
		}
	}
}
