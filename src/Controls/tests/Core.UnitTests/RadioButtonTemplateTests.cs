using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[Category("RadioButton")]
	public class RadioButtonTemplateTests : BaseTestFixture
	{
		public class BorderStyleCases : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] { Border.VerticalOptionsProperty, LayoutOptions.End };
				yield return new object[] { Border.HorizontalOptionsProperty, LayoutOptions.End };
				yield return new object[] { Border.StrokeProperty, Colors.Magenta };
				yield return new object[] { Border.StrokeShapeProperty, new RoundRectangle() };
				yield return new object[] { Border.StrokeThicknessProperty, new Thickness(1, 2, 3, 4) };
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		[ClassData(typeof(BorderStyleCases))]
		[Theory(DisplayName = "Border Style properties should not affect RadioButton")]
		public void RadioButtonIgnoresBorderStyleProperties(BindableProperty property, object value)
		{
			var implicitBorderStyle = new Style(typeof(Border));
			implicitBorderStyle.Setters.Add(new Setter() { Property = property, Value = value });

			var page = new ContentPage();
			page.Resources.Add(implicitBorderStyle);

			var radioButton = new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate };
			page.Content = radioButton;

			var root = (radioButton as IControlTemplated)?.TemplateRoot as Border;

			Assert.NotNull(root);
			Assert.NotEqual(value, root.GetValue(property));
		}

		class RadioButtonStyleCases : IEnumerable<object[]>
		{
			public IEnumerator<object[]> GetEnumerator()
			{
				yield return new object[] { RadioButton.VerticalOptionsProperty, LayoutOptions.End };
				yield return new object[] { RadioButton.HorizontalOptionsProperty, LayoutOptions.End };
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

			var root = (radioButton as IControlTemplated)?.TemplateRoot as Border;

			Assert.NotNull(root);
			Assert.Equal(root.GetValue(property), value);
		}

		[Fact]
		public void TemplateFromAppLevelImplicitStyleAddsTapGestureToCompositeRecognizers()
		{
			// https://github.com/dotnet/maui/issues/14949
			// An application-level implicit style applies the ControlTemplate while the
			// RadioButton is still being constructed. The tap gesture added during
			// OnApplyTemplate must still end up in CompositeGestureRecognizers, which is
			// the collection the iOS/MacCatalyst gesture manager attaches to.
			ApplicationExtensions.CreateAndSetMockApplication();
			try
			{
				var implicitStyle = new Style(typeof(RadioButton));
				implicitStyle.Setters.Add(new Setter
				{
					Property = TemplatedView.ControlTemplateProperty,
					Value = new ControlTemplate(() => new Grid { new ContentPresenter() })
				});
				Application.Current.Resources.Add(implicitStyle);

				var radioButton = new RadioButton();

				Assert.NotNull(radioButton.ControlTemplate);
				var tapGesture = Assert.Single(radioButton.GestureRecognizers.OfType<TapGestureRecognizer>());
				var compositeTapGesture = Assert.Single(((IGestureController)radioButton).CompositeGestureRecognizers.OfType<TapGestureRecognizer>());
				Assert.Same(tapGesture, compositeTapGesture);
				Assert.Same(radioButton, tapGesture.Parent);
			}
			finally
			{
				Application.ClearCurrent();
			}
		}

		[Fact]
		public void BorderSpecificRadioButtonStyleSetsPropertyOnTemplateRoot()
		{
			var borderColor = Colors.Magenta;
			var borderWidthProperty = 4.3;
			var cornerRadiusProperty = 5;
			var radioButtonStyle = new Style(typeof(RadioButton));

			radioButtonStyle.Setters.Add(new Setter() { Property = RadioButton.BorderColorProperty, Value = borderColor });
			radioButtonStyle.Setters.Add(new Setter() { Property = RadioButton.BorderWidthProperty, Value = borderWidthProperty });
			radioButtonStyle.Setters.Add(new Setter() { Property = RadioButton.CornerRadiusProperty, Value = cornerRadiusProperty });

			var radioButton = new RadioButton() { ControlTemplate = RadioButton.DefaultTemplate, Style = radioButtonStyle };
			var root = (radioButton as IControlTemplated)?.TemplateRoot as Border;

			Assert.NotNull(root);
			Assert.Equal(root.GetValue(Border.StrokeProperty), Brush.Magenta);
			Assert.Equal(root.GetValue(Border.StrokeThicknessProperty), borderWidthProperty);

			RoundRectangle rec = (RoundRectangle)root.StrokeShape;
			Assert.Equal(rec.CornerRadius, cornerRadiusProperty);
		}
	}
}
