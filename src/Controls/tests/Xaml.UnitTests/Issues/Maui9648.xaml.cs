using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Issues;

// Custom Entry class for testing derived type styles
public class CustomEntry : Entry
{
}

public partial class Maui9648 : ContentPage
{
	public Maui9648() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] 
		public void SetUp() => Application.Current = new MockApplication();

		[Test]
		public void ApplyToDerivedTypesWorksForMultipleImplicitStyles([Values] XamlInflator inflator)
		{
			var page = new Maui9648(inflator);
			Application.Current.LoadPage(page);

			// CustomEntry should have all three styles applied
			Assert.That(page.testEntry.TextColor, Is.EqualTo(Colors.Red), 
				"CustomEntry should have TextColor from InputView implicit style");
			Assert.That(page.testEntry.FontSize, Is.EqualTo(48), 
				"CustomEntry should have FontSize from Entry implicit style");
			Assert.That(page.testEntry.BackgroundColor, Is.EqualTo(Colors.LightGreen), 
				"CustomEntry should have BackgroundColor from its own implicit style");
		}

		[Test]
		public void ApplyToDerivedTypesWorksForEntry([Values] XamlInflator inflator)
		{
			var page = new Maui9648(inflator);
			Application.Current.LoadPage(page);

			// Regular Entry should have styles from InputView and Entry
			Assert.That(page.regularEntry.TextColor, Is.EqualTo(Colors.Red), 
				"Entry should have TextColor from InputView implicit style");
			Assert.That(page.regularEntry.FontSize, Is.EqualTo(48), 
				"Entry should have FontSize from its own implicit style");
		}
	}
}
