using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui12550 : ContentPage
{
	public Maui12550() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void CSSStyleAppliedToInitiallyEnabledButton([Values] XamlInflator inflator)
		{
			var page = new Maui12550(inflator);
			Assert.That(page.EnabledButton.BackgroundColor, Is.EqualTo(Colors.Green));
		}

		[Test]
		public void CSSStyleAppliedAfterReEnablingInitiallyDisabledButton([Values] XamlInflator inflator)
		{
			var page = new Maui12550(inflator);
			Assert.That(page.DisabledButton.IsEnabled, Is.False);

			page.DisabledButton.IsEnabled = true;

			// https://github.com/dotnet/maui/issues/12550
			Assert.That(page.DisabledButton.BackgroundColor, Is.EqualTo(Colors.Green),
				"CSS background-color should be applied after re-enabling a button that was initially disabled");
		}

		[Test]
		public void CSSStyleAppliedAfterDisableEnableCycle([Values] XamlInflator inflator)
		{
			var page = new Maui12550(inflator);
			Assert.That(page.EnabledButton.BackgroundColor, Is.EqualTo(Colors.Green));

			page.EnabledButton.IsEnabled = false;
			page.EnabledButton.IsEnabled = true;

			Assert.That(page.EnabledButton.BackgroundColor, Is.EqualTo(Colors.Green),
				"CSS background-color should be preserved after disable/enable cycle");
		}
	}
}
