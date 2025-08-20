using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui11204 : ContentPage
{
	public Maui11204() => InitializeComponent();

	class Tests
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void VSMSetterOverrideManualValues([Values] XamlInflator inflator)
		{
			var page = new Maui11204(inflator);
			Assert.AreEqual(Colors.FloralWhite, page.border.BackgroundColor);
			VisualStateManager.GoToState(page.border, "State1");
			Assert.AreEqual(2, page.border.StrokeThickness);
			Assert.AreEqual(Colors.Blue, page.border.BackgroundColor);
		}

		[Test]
		public void StyleVSMSetterOverrideManualValues([Values] XamlInflator inflator)
		{
			var page = new Maui11204(inflator);
			Assert.AreEqual(Colors.HotPink, page.borderWithStyleClass.BackgroundColor);
			VisualStateManager.GoToState(page.borderWithStyleClass, "State1");
			Assert.AreEqual(2, page.borderWithStyleClass.StrokeThickness);
			Assert.AreEqual(Colors.Blue, page.borderWithStyleClass.BackgroundColor);
		}
	}
}