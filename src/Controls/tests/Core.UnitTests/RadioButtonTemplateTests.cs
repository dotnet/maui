using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[Category("RadioButton")]
	public class RadioButtonTemplateTests : BaseTestFixture
	{
		public class FrameStyleCases : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] { Frame.VerticalOptionsProperty, LayoutOptions.End };
				yield return new object[] { Frame.HorizontalOptionsProperty, LayoutOptions.End };
				yield return new object[] { Frame.BackgroundColorProperty, Colors.Red };
				yield return new object[] { Frame.BorderColorProperty, Colors.Magenta };
				yield return new object[] { Frame.MarginProperty, new Thickness(1, 2, 3, 4) };
				yield return new object[] { Frame.OpacityProperty, 0.67 };
				yield return new object[] { Frame.RotationProperty, 0.3 };
				yield return new object[] { Frame.ScaleProperty, 0.8 };
				yield return new object[] { Frame.ScaleXProperty, 0.9 };
				yield return new object[] { Frame.ScaleYProperty, 0.95 };
				yield return new object[] { Frame.TranslationXProperty, 123 };
				yield return new object[] { Frame.TranslationYProperty, 321 };
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		[ClassData(typeof(FrameStyleCases))]
		[Theory(DisplayName = "Frame Style properties should not affect RadioButton")]
		public void RadioButtonIgnoresFrameStyleProperties(BindableProperty property, object value)
		{
			var implicitFrameStyle = new Style(typeof(Frame));
			implicitFrameStyle.Setters.Add(new Setter() { Property = property, Value = value });

			var page = new ContentPage();
			page.Resources.Add(implicitFrameStyle);

			var radioButton = new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate };
			page.Content = radioButton;

			var root = (radioButton as IControlTemplated)?.TemplateRoot as Frame;

			Assert.NotNull(root);
			Assert.NotEqual(value, root.GetValue(property));
		}

		class RadioButtonStyleCases : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] { RadioButton.VerticalOptionsProperty, LayoutOptions.End };
				yield return new object[] { RadioButton.HorizontalOptionsProperty, LayoutOptions.End };
				yield return new object[] { RadioButton.BackgroundColorProperty, Colors.Red };
				yield return new object[] { RadioButton.MarginProperty, new Thickness(1, 2, 3, 4) };
				yield return new object[] { RadioButton.OpacityProperty, 0.67 };
				yield return new object[] { RadioButton.RotationProperty, 0.3 };
				yield return new object[] { RadioButton.ScaleProperty, 0.8 };
				yield return new object[] { RadioButton.ScaleXProperty, 0.9 };
				yield return new object[] { RadioButton.ScaleYProperty, 0.95 };
				yield return new object[] { RadioButton.TranslationXProperty, 123d };
				yield return new object[] { RadioButton.TranslationYProperty, 321d };
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		[ClassData(typeof(RadioButtonStyleCases))]
		[Theory(DisplayName = "RadioButton Style properties should affect RadioButton")]
		public void RadioButtonStyleSetsPropertyOnTemplateRoot(BindableProperty property, object value)
		{
			var radioButtonStyle = new Style(typeof(RadioButton));
			radioButtonStyle.Setters.Add(new Setter() { Property = property, Value = value });

			var radioButton = new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate, Style = radioButtonStyle };

			var root = (radioButton as IControlTemplated)?.TemplateRoot as Frame;

			Assert.NotNull(root);
			Assert.Equal(root.GetValue(property), value);
		}
	}
}
