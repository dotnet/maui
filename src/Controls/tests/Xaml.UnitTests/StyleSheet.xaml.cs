using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;
using Xunit;

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
		}		public class Tests
		{
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
				Assert.Equal(Colors.Azure, layout.label0.TextColor);
				Assert.Equal(Colors.AliceBlue, layout.label0.BackgroundColor);
			}
		}
	}
}