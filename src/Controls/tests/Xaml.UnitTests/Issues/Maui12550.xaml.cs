using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui12550 : ContentPage
{
	public Maui12550() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void CSSStyleAppliedToInitiallyEnabledButton(XamlInflator inflator)
		{
			var page = new Maui12550(inflator);
			Assert.Equal(Colors.Green, page.EnabledButton.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void CSSStyleAppliedAfterReEnablingInitiallyDisabledButton(XamlInflator inflator)
		{
			var page = new Maui12550(inflator);
			Assert.False(page.DisabledButton.IsEnabled);

			page.DisabledButton.IsEnabled = true;

			// https://github.com/dotnet/maui/issues/12550
			Assert.Equal(Colors.Green, page.DisabledButton.BackgroundColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void CSSStyleAppliedAfterDisableEnableCycle(XamlInflator inflator)
		{
			var page = new Maui12550(inflator);
			Assert.Equal(Colors.Green, page.EnabledButton.BackgroundColor);

			page.EnabledButton.IsEnabled = false;
			page.EnabledButton.IsEnabled = true;

			Assert.Equal(Colors.Green, page.EnabledButton.BackgroundColor);
		}
	}
}
