using System;
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

	}
}