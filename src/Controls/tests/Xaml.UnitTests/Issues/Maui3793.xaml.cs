using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3793 : ContentPage
{
	public Maui3793() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void ControlTemplateFromStyle([Values] XamlInflator inflator)
		{
			Maui3793 page;
			Assert.DoesNotThrow(() => page = new Maui3793(inflator));
		}
	}
}
