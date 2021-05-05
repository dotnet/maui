using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
				Microsoft.Maui.Controls.Internals.Registrar.RegisterAll(new Type[0]);
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
				Assert.That(layout.label0.TextColor, Is.EqualTo(Colors.Azure));
				Assert.That(layout.label0.BackgroundColor, Is.EqualTo(Colors.AliceBlue));
			}
		}
	}
}