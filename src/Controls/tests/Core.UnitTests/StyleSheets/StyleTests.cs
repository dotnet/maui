using System;
using System.IO;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{
	[TestFixture]
	public class StyleTests
	{
		[SetUp]
		public void SetUp()
		{
			Device.PlatformServices = new MockPlatformServices();
			Internals.Registrar.RegisterAll(new Type[0]);
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
			Application.ClearCurrent();
		}

		[Test]
		public void PropertiesAreApplied()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var ve = new VisualElement();
			Assume.That(ve.BackgroundColor, Is.EqualTo(null));
			style.Apply(ve);
			Assert.That(ve.BackgroundColor, Is.EqualTo(Colors.Red));
		}

		[Test]
		public void PropertiesSetByStyleDoesNotOverrideManualOne()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var ve = new VisualElement() { BackgroundColor = Colors.Pink };
			Assume.That(ve.BackgroundColor, Is.EqualTo(Colors.Pink));

			style.Apply(ve);
			Assert.That(ve.BackgroundColor, Is.EqualTo(Colors.Pink));
		}

		[Test]
		public void StylesAreCascading()
		{
			//color should cascade, background-color should not
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var label = new Label();
			var layout = new StackLayout
			{
				Children = {
					label,
				}
			};

			Assume.That(layout.BackgroundColor, Is.EqualTo(null));
			Assume.That(label.BackgroundColor, Is.EqualTo(null));
			Assume.That(label.TextColor, Is.EqualTo(null));

			style.Apply(layout);
			Assert.That(layout.BackgroundColor, Is.EqualTo(Colors.Red));
			Assert.That(label.BackgroundColor, Is.EqualTo(null));
			Assert.That(label.TextColor, Is.EqualTo(Colors.Lime));
		}

		[Test]
		public void PropertiesAreOnlySetOnMatchingElements()
		{
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var layout = new StackLayout();
			Assert.That(layout.GetValue(TextElement.TextColorProperty), Is.EqualTo(null));
		}

		[Test]
		public void StyleSheetsOnAppAreApplied()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: red;}"));
			var page = new ContentPage
			{
				Content = new Label()
			};
			app.MainPage = page;
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Colors.Red));
		}

		[Test]
		public void StyleSheetsOnAppAreAppliedBeforePageStyleSheet()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: white; background-color: blue; }"));
			var page = new ContentPage
			{
				Content = new Label()
			};
			page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
			app.MainPage = page;
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Colors.Red));
			Assert.That((page.Content as Label).BackgroundColor, Is.EqualTo(Colors.Blue));
		}

		[Test]
		public void StyleSheetsOnChildAreReAppliedWhenParentStyleSheetAdded()
		{
			var app = new MockApplication();
			var page = new ContentPage
			{
				Content = new Label()
			};
			page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
			app.MainPage = page;
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Colors.Red));

			app.Resources.Add(StyleSheet.FromString("label{ color: white; background-color: blue; }"));
			Assert.That((page.Content as Label).BackgroundColor, Is.EqualTo(Colors.Blue));
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Colors.Red));
		}

		[Test]
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
			app.MainPage = page;
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Colors.Yellow));
		}

	}
}