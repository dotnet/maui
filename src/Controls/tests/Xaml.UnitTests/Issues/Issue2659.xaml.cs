using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2659 : ContentPage
{
	public Issue2659() => InitializeComponent();

	void OnSetStyleButtonClicked(object sender, EventArgs args)
	{
		Style style = (Style)Resources["buttonStyle"];
		SetButtonStyle(style);
	}

	void OnUnsetStyleButtonClicked(object sender, EventArgs args) => SetButtonStyle(null);

	void OnSetLocalButtonClicked(object sender, EventArgs args)
	{
		EnumerateButtons((Button button) =>
		{
			button.TextColor = Colors.Red;
			button.FontAttributes = FontAttributes.Bold;
		});
	}

	void OnClearLocalButtonClicked(object sender, EventArgs args)
	{
		EnumerateButtons((Button button) =>
		{
			button.ClearValue(Button.TextColorProperty);
			button.ClearValue(Button.FontAttributesProperty);
		});
	}

	void SetButtonStyle(Style style)
	{
		EnumerateButtons(button =>
		{
			button.Style = style;
		});
	}

	void EnumerateButtons(Action<Button> action)
	{
		foreach (View view in stackLayout.Children)
			action((Button)view);
	}


	public class Tests
	{
		void AssertStyleApplied(Button button)
		{
			Assert.Equal(LayoutOptions.Center, button.HorizontalOptions);
			Assert.Equal(LayoutOptions.CenterAndExpand, button.VerticalOptions);
			Assert.Equal(16, button.FontSize);
			Assert.Equal(Colors.Blue, button.TextColor);
			Assert.Equal(FontAttributes.Italic, button.FontAttributes);
		}

		void AssertStyleUnApplied(Button button)
		{
			Assert.Equal(View.HorizontalOptionsProperty.DefaultValue, button.HorizontalOptions);
			Assert.Equal(View.VerticalOptionsProperty.DefaultValue, button.VerticalOptions);
			Assert.Equal(new Button().GetDefaultFontSize(), button.FontSize);
			Assert.Equal(Button.TextColorProperty.DefaultValue, button.TextColor);
			Assert.Equal(Button.FontAttributesProperty.DefaultValue, button.FontAttributes);
		}

		[Theory]
		[Values]
		public void SetUnsetStyleFromResource(XamlInflator inflator)
		{
			var layout = new Issue2659(inflator);
			layout.EnumerateButtons(AssertStyleUnApplied);

			((IButtonController)layout.button0).SendClicked();
			layout.EnumerateButtons(AssertStyleApplied);

			((IButtonController)layout.button1).SendClicked();
			layout.EnumerateButtons(AssertStyleUnApplied);
		}

		void AssertPropertiesApplied(Button button)
		{
			Assert.Equal(Colors.Red, button.TextColor);
			Assert.Equal(FontAttributes.Bold, button.FontAttributes);
		}

		void AssertPropertiesUnApplied(Button button)
		{
			Assert.Equal(Button.TextColorProperty.DefaultValue, button.TextColor);
			Assert.Equal(Button.FontAttributesProperty.DefaultValue, button.FontAttributes);
		}

		[Theory]
		[Values]
		public void SetUnsetLocalProperties(XamlInflator inflator)
		{
			var layout = new Issue2659(inflator);
			layout.EnumerateButtons(AssertPropertiesUnApplied);

			((IButtonController)layout.button2).SendClicked();
			layout.EnumerateButtons(AssertPropertiesApplied);

			((IButtonController)layout.button3).SendClicked();
			layout.EnumerateButtons(AssertPropertiesUnApplied);
		}
	}
}