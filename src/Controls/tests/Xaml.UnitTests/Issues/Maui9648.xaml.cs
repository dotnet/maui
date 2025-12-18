using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Issues;

// Custom Entry class for testing derived type styles
public class CustomEntry : Entry
{
}

public partial class Maui9648 : ContentPage
{
	public Maui9648() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = new MockApplication();
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void ApplyToDerivedTypesWorksForMultipleImplicitStyles(XamlInflator inflator)
		{
			var page = new Maui9648(inflator);
			Application.Current.LoadPage(page);

			// CustomEntry should have all three styles applied
			Assert.Equal(Colors.Red, page.testEntry.TextColor);
			Assert.Equal(48, page.testEntry.FontSize);
			Assert.Equal(Colors.LightGreen, page.testEntry.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void ApplyToDerivedTypesWorksForEntry(XamlInflator inflator)
		{
			var page = new Maui9648(inflator);
			Application.Current.LoadPage(page);

			// Regular Entry should have styles from InputView and Entry
			Assert.Equal(Colors.Red, page.regularEntry.TextColor);
			Assert.Equal(48, page.regularEntry.FontSize);
		}
	}
}
