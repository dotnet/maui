using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class StyleSheet : ContentPage
	{
		public StyleSheet()
		{
			InitializeComponent();
		}

		public StyleSheet(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
				Xamarin.Forms.Internals.Registrar.RegisterAll(new Type[0]);
			}

			[TestCase(false), TestCase(true)]
			public void EmbeddedStyleSheetsAreLoaded(bool useCompiledXaml)
			{
				var layout = new StyleSheet(useCompiledXaml);
				Assert.That(layout.Resources.StyleSheets[0].Styles.Count, Is.GreaterThanOrEqualTo(1));
			}

			[TestCase(false), TestCase(true)]
			public void StyleSheetsAreApplied(bool useCompiledXaml)
			{
				var layout = new StyleSheet(useCompiledXaml);
				Assert.That(layout.label0.TextColor, Is.EqualTo(Color.Azure));
				Assert.That(layout.label0.BackgroundColor, Is.EqualTo(Color.AliceBlue));
			}
		}
	}
}