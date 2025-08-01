using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22536
{
	public Maui22536() => InitializeComponent();

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
		public void TestNonCompiledResourceDictionary([Values] XamlInflator inflator)
		{
			var page = new Maui22536(inflator);
			Assert.AreEqual(page.Button.BackgroundColor, Color.FromArgb("#010203"));
		}
	}
}