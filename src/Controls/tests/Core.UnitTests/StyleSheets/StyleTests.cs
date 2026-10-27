using System;
using System.Globalization;
using System.IO;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class StyleTests : BaseTestFixture
	{
		public StyleTests()
		{
			ApplicationExtensions.CreateAndSetMockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Application.ClearCurrent();
			}

			base.Dispose(disposing);
		}

		[Fact]
		public void PropertiesAreApplied()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);

			var ve = new VisualElement();
			Assert.Null(ve.BackgroundColor);
			style.Apply(ve);
			Assert.Equal(ve.BackgroundColor, Colors.Red);
		}

		[Fact]
		public void PropertiesSetByStyleDoesNotOverrideManualOne()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);

			var ve = new VisualElement() { BackgroundColor = Colors.Pink };
			Assert.Equal(ve.BackgroundColor, Colors.Pink);

			style.Apply(ve);
			Assert.Equal(ve.BackgroundColor, Colors.Pink);
		}

		[Fact]
		public void StylesAreCascading()
		{
			//color should cascade, background-color should not
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);

			var label = new Label();
			var layout = new StackLayout
			{
				Children = {
					label,
				}
			};

			Assert.Null(layout.BackgroundColor);
			Assert.Null(label.BackgroundColor);
			Assert.Null(label.TextColor);

			style.Apply(layout);
			Assert.Equal(layout.BackgroundColor, Colors.Red);
			Assert.Null(label.BackgroundColor);
			Assert.Equal(label.TextColor, Colors.Lime);
		}

		[Fact]
		public void PropertiesAreOnlySetOnMatchingElements()
		{
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);

			var layout = new StackLayout();
			Assert.Null(layout.GetValue(TextElement.TextColorProperty));
		}

		[Fact]
		public void StyleSheetsOnAppAreApplied()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: red;}"));
			var page = new ContentPage
			{
				Content = new Label()
			};
			app.LoadPage(page);
			Assert.Equal((page.Content as Label).TextColor, Colors.Red);
		}

		[Fact]
		public void StyleSheetsOnAppAreAppliedBeforePageStyleSheet()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: white; background-color: blue; }"));
			var page = new ContentPage
			{
				Content = new Label()
			};
			page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
			app.LoadPage(page);
			Assert.Equal((page.Content as Label).TextColor, Colors.Red);
			Assert.Equal((page.Content as Label).BackgroundColor, Colors.Blue);
		}

		[Fact]
		public void StyleSheetsOnChildAreReAppliedWhenParentStyleSheetAdded()
		{
			var app = new MockApplication();
			var page = new ContentPage
			{
				Content = new Label()
			};
			page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
			app.LoadPage(page);
			Assert.Equal((page.Content as Label).TextColor, Colors.Red);

			app.Resources.Add(StyleSheet.FromString("label{ color: white; background-color: blue; }"));
			Assert.Equal((page.Content as Label).BackgroundColor, Colors.Blue);
			Assert.Equal((page.Content as Label).TextColor, Colors.Red);
		}

		[Fact]
		public void StyleSheetsOnSubviewAreAppliedBeforePageStyleSheet()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: white; }"));
			var label = new Label();
			label.Resources.Add(StyleSheet.FromString("label{color: yellow;}"));

			var page = new ContentPage
			{
				Content = label
			};
			page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
			app.LoadPage(page);
			Assert.Equal((page.Content as Label).TextColor, Colors.Yellow);
		}

		[Fact]
		public void CSSStyleAppliedAfterReEnablingInitiallyDisabledButton_Issue12550()
		{
			// https://github.com/dotnet/maui/issues/12550
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("button{ background-color: green; }"));

			var button = new Button { IsEnabled = false };
			var page = new ContentPage { Content = button };
			app.LoadPage(page);

			button.IsEnabled = true;

			Assert.Equal(Colors.Green, button.BackgroundColor);
		}

		[Theory]
		[InlineData("en-US")]
		[InlineData("de-DE")]
		[InlineData("fr-FR")]
		public void TransformValuesUseInvariantCulture(string culture)
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

			var styleString = @"transform: translate(10.5,20.25) scale(1.5,1.5) rotate(45.5);";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);

			var ve = new VisualElement();
			style.Apply(ve);

			Assert.Equal(10.5, ve.TranslationX);
			Assert.Equal(20.25, ve.TranslationY);
			Assert.Equal(1.5, ve.ScaleX);
			Assert.Equal(1.5, ve.ScaleY);
			Assert.Equal(45.5, ve.Rotation);
		}

		[Theory]
		[InlineData("en-US")]
		[InlineData("de-DE")]
		public void SingleAxisTransformValuesUseInvariantCulture(string culture)
		{
			CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);

			var styleString = @"transform: translateX(1.5) translateY(2.5) scaleX(0.5) scaleY(0.75) rotateX(12.5) rotateY(22.5);";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);

			var ve = new VisualElement();
			style.Apply(ve);

			Assert.Equal(1.5, ve.TranslationX);
			Assert.Equal(2.5, ve.TranslationY);
			Assert.Equal(0.5, ve.ScaleX);
			Assert.Equal(0.75, ve.ScaleY);
			Assert.Equal(12.5, ve.RotationX);
			Assert.Equal(22.5, ve.RotationY);
		}

	}
}