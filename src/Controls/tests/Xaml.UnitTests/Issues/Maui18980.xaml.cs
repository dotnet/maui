using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui18980 : ContentPage
{
	public Maui18980() => InitializeComponent();

	[TestFixture]
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
		public void CSSnotOverridenbyImplicitStyle([Values] XamlInflator inflator)
		{
			// var app = new MockApplication();
			// app.Resources.Add(new Maui18980Style(inflator));
			// Application.SetCurrentApplication(app);

			var page = new Maui18980(inflator);
			Assert.That(page.button.BackgroundColor, Is.EqualTo(Colors.Red));
		}
	}
}
