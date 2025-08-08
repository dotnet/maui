using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui13474
{
	public Maui13474() => InitializeComponent();

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
		public void FontImageSourceIsAppliedFromSharedResources([Values] XamlInflator inflator)
		{
			var page = new Maui13474(inflator);
			var fontImageSource = page.imageButton.Source as FontImageSource;
			Assert.AreEqual(fontImageSource.Color, Colors.Red);
			Assert.AreEqual(fontImageSource.FontFamily, "FontAwesome");
		}
	}
}
