using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class Issue2659 : ContentPage
	{
		public Issue2659()
		{
			InitializeComponent();
		}

		public Issue2659(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		void OnSetStyleButtonClicked(object sender, EventArgs args)
		{
			Style style = (Style)Resources["buttonStyle"];
			SetButtonStyle(style);
		}

		void OnUnsetStyleButtonClicked(object sender, EventArgs args)
		{
			SetButtonStyle(null);
		}

		void OnSetLocalButtonClicked(object sender, EventArgs args)
		{
			EnumerateButtons((Button button) =>
			{
				button.TextColor = Color.Red;
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

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			void AssertStyleApplied(Button button)
			{
				Assert.AreEqual(LayoutOptions.Center, button.HorizontalOptions);
				Assert.AreEqual(LayoutOptions.CenterAndExpand, button.VerticalOptions);
				Assert.AreEqual(16, button.FontSize);
				Assert.AreEqual(Color.Blue, button.TextColor);
				Assert.AreEqual(FontAttributes.Italic, button.FontAttributes);
			}

			void AssertStyleUnApplied(Button button)
			{
				Assert.AreEqual(View.HorizontalOptionsProperty.DefaultValue, button.HorizontalOptions);
				Assert.AreEqual(View.VerticalOptionsProperty.DefaultValue, button.VerticalOptions);
				Assert.AreEqual(10, button.FontSize);
				Assert.AreEqual(Button.TextColorProperty.DefaultValue, button.TextColor);
				Assert.AreEqual(Button.FontAttributesProperty.DefaultValue, button.FontAttributes);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void SetUnsetStyleFromResource(bool useCompiledXaml)
			{
				var layout = new Issue2659(useCompiledXaml);
				layout.EnumerateButtons(AssertStyleUnApplied);

				((IButtonController)layout.button0).SendClicked();
				layout.EnumerateButtons(AssertStyleApplied);

				((IButtonController)layout.button1).SendClicked();
				layout.EnumerateButtons(AssertStyleUnApplied);
			}

			void AssertPropertiesApplied(Button button)
			{
				Assert.AreEqual(Color.Red, button.TextColor);
				Assert.AreEqual(FontAttributes.Bold, button.FontAttributes);
			}

			void AssertPropertiesUnApplied(Button button)
			{
				Assert.AreEqual(Button.TextColorProperty.DefaultValue, button.TextColor);
				Assert.AreEqual(Button.FontAttributesProperty.DefaultValue, button.FontAttributes);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void SetUnsetLocalProperties(bool useCompiledXaml)
			{
				var layout = new Issue2659(useCompiledXaml);
				layout.EnumerateButtons(AssertPropertiesUnApplied);

				((IButtonController)layout.button2).SendClicked();
				layout.EnumerateButtons(AssertPropertiesApplied);

				((IButtonController)layout.button3).SendClicked();
				layout.EnumerateButtons(AssertPropertiesUnApplied);
			}
		}
	}
}