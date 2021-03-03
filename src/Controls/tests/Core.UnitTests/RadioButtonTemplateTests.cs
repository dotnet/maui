using System;
using System.Collections;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture(Category = "RadioButton")]
	public class RadioButtonTemplateTests : BaseTestFixture
	{
		class FrameStyleCases : IEnumerable
		{
			public IEnumerator GetEnumerator()
			{
				yield return new object[] { Frame.VerticalOptionsProperty, LayoutOptions.End };
				yield return new object[] { Frame.HorizontalOptionsProperty, LayoutOptions.End };
				yield return new object[] { Frame.BackgroundColorProperty, Color.Red };
				yield return new object[] { Frame.BorderColorProperty, Color.Magenta };
				yield return new object[] { Frame.MarginProperty, new Thickness(1, 2, 3, 4) };
				yield return new object[] { Frame.OpacityProperty, 0.67 };
				yield return new object[] { Frame.RotationProperty, 0.3 };
				yield return new object[] { Frame.ScaleProperty, 0.8 };
				yield return new object[] { Frame.ScaleXProperty, 0.9 };
				yield return new object[] { Frame.ScaleYProperty, 0.95 };
				yield return new object[] { Frame.TranslationXProperty, 123 };
				yield return new object[] { Frame.TranslationYProperty, 321 };
			}
		}

		[TestCaseSource(typeof(FrameStyleCases))]
		[Description("Frame Style properties should not affect RadioButton")]
		public void RadioButtonIgnoresFrameStyleProperties(BindableProperty property, object value)
		{
			var implicitFrameStyle = new Style(typeof(Frame));
			implicitFrameStyle.Setters.Add(new Setter() { Property = property, Value = value });

			var page = new ContentPage();
			page.Resources.Add(implicitFrameStyle);

			var radioButton = new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate };
			page.Content = radioButton;

			var root = (radioButton as IControlTemplated)?.TemplateRoot as Frame;

			Assert.IsNotNull(root);
			Assert.That(root.GetValue(property), Is.Not.EqualTo(value), $"{property.PropertyName} should be ignored.");
		}

		class RadioButtonStyleCases : IEnumerable
		{
			public IEnumerator GetEnumerator()
			{
				yield return new object[] { RadioButton.VerticalOptionsProperty, LayoutOptions.End };
				yield return new object[] { RadioButton.HorizontalOptionsProperty, LayoutOptions.End };
				yield return new object[] { RadioButton.BackgroundColorProperty, Color.Red };
				yield return new object[] { RadioButton.MarginProperty, new Thickness(1, 2, 3, 4) };
				yield return new object[] { RadioButton.OpacityProperty, 0.67 };
				yield return new object[] { RadioButton.RotationProperty, 0.3 };
				yield return new object[] { RadioButton.ScaleProperty, 0.8 };
				yield return new object[] { RadioButton.ScaleXProperty, 0.9 };
				yield return new object[] { RadioButton.ScaleYProperty, 0.95 };
				yield return new object[] { RadioButton.TranslationXProperty, 123 };
				yield return new object[] { RadioButton.TranslationYProperty, 321 };
			}
		}

		[TestCaseSource(typeof(RadioButtonStyleCases))]
		[Description("RadioButton Style properties should affect RadioButton")]
		public void RadioButtonStyleSetsPropertyOnTemplateRoot(BindableProperty property, object value)
		{
			var radioButtonStyle = new Style(typeof(RadioButton));
			radioButtonStyle.Setters.Add(new Setter() { Property = property, Value = value });

			var radioButton = new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate, Style = radioButtonStyle };

			var root = (radioButton as IControlTemplated)?.TemplateRoot as Frame;

			Assert.IsNotNull(root);
			Assert.That(root.GetValue(property), Is.EqualTo(value), $"{property.PropertyName} should match.");
		}
	}
}
