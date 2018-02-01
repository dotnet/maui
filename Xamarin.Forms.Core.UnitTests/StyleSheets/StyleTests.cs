using System;
using System.IO;

using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.StyleSheets.UnitTests
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
			Assume.That(ve.BackgroundColor, Is.EqualTo(Color.Default));
			style.Apply(ve);
			Assert.That(ve.BackgroundColor, Is.EqualTo(Color.Red));
		}

		[Test]
		public void PropertiesSetByStyleDoesNotOverrideManualOne()
		{
			var styleString = @"background-color: #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var ve = new VisualElement() { BackgroundColor = Color.Pink };
			Assume.That(ve.BackgroundColor, Is.EqualTo(Color.Pink));

			style.Apply(ve);
			Assert.That(ve.BackgroundColor, Is.EqualTo(Color.Pink));
		}

		[Test]
		public void StylesAreCascading()
		{
			//color should cascade, background-color should not
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var label = new Label();
			var layout = new StackLayout {
				Children = {
					label,
				}
			};

			Assume.That(layout.BackgroundColor, Is.EqualTo(Color.Default));
			Assume.That(label.BackgroundColor, Is.EqualTo(Color.Default));
			Assume.That(label.TextColor, Is.EqualTo(Color.Default));

			style.Apply(layout);
			Assert.That(layout.BackgroundColor, Is.EqualTo(Color.Red));
			Assert.That(label.BackgroundColor, Is.EqualTo(Color.Default));
			Assert.That(label.TextColor, Is.EqualTo(Color.Lime));
		}

		[Test]
		public void PropertiesAreOnlySetOnMatchingElements()
		{
			var styleString = @"background-color: #ff0000; color: #00ff00;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assume.That(style, Is.Not.Null);

			var layout = new StackLayout();
			Assert.That(layout.GetValue(TextElement.TextColorProperty), Is.EqualTo(Color.Default));
		}

		[Test]
		public void StyleSheetsOnAppAreApplied()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label{ color: red;}"));
			var page = new ContentPage {
				Content = new Label()
			};
			app.MainPage = page;
			Assert.That((page.Content as Label).TextColor, Is.EqualTo(Color.Red));
		}
	}
}