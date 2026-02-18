using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	/// <summary>
	/// Tests that validate Span font property resolution when inheriting from a Label's default font.
	///
	/// The font construction logic in FormattedStringExtensions (Android, iOS, Windows) uses only
	/// cross-platform APIs: Span.IsSet(), Font.OfSize(), .WithAttributes(). These tests validate
	/// that the resolution pattern produces correct Font values for all property combinations.
	///
	/// Related: https://github.com/dotnet/maui/issues/21326
	/// Related: https://github.com/dotnet/maui/pull/34110
	/// </summary>
	public class SpanFontInheritanceTests : BaseTestFixture
	{
		/// <summary>
		/// Replicates the font resolution logic from PR #34110's FormattedStringExtensions.
		/// This is the exact same code on all three platforms (Android, iOS, Windows).
		/// </summary>
		static Font BuildFontUsingPR34110Logic(Span span, Font? defaultFont, double defaultFontSize)
		{
			var fontFamily = span.IsSet(Span.FontFamilyProperty) ? span.FontFamily : defaultFont?.Family;
			var fontSize = span.FontSize >= 0 ? span.FontSize : defaultFontSize;
			return Font.OfSize(fontFamily, fontSize);
		}

		/// <summary>
		/// The complete font resolution that preserves all font properties with per-property inheritance.
		/// This is the suggested fix for PR #34110.
		/// </summary>
		static Font BuildFontWithCompleteInheritance(Span span, Font? defaultFont, double defaultFontSize)
		{
			var fontFamily = span.IsSet(Span.FontFamilyProperty) ? span.FontFamily : defaultFont?.Family;
			var fontSize = span.IsSet(Span.FontSizeProperty) ? span.FontSize : defaultFontSize;
			var fontAttributes = span.IsSet(Span.FontAttributesProperty)
				? span.FontAttributes
				: (defaultFont?.GetFontAttributes() ?? FontAttributes.None);
			var autoScaling = span.IsSet(Span.FontAutoScalingEnabledProperty)
				? span.FontAutoScalingEnabled
				: (defaultFont?.AutoScalingEnabled ?? true);
			return Font.OfSize(fontFamily, fontSize, enableScaling: autoScaling).WithAttributes(fontAttributes);
		}

		// =====================================================================
		// FontFamily inheritance (the fix PR #34110 targets — should work)
		// =====================================================================

		[Fact]
		public void FontFamily_InheritedFromDefaultFont_WhenNotSetOnSpan()
		{
			var span = new Span { Text = "Inherit family" };
			var defaultFont = Font.OfSize("Montserrat", 20);

			var font = BuildFontUsingPR34110Logic(span, defaultFont, 20);

			Assert.Equal("Montserrat", font.Family);
		}

		[Fact]
		public void FontFamily_UsesSpanValue_WhenExplicitlySet()
		{
			var span = new Span { Text = "Own family", FontFamily = "Arial" };
			var defaultFont = Font.OfSize("Montserrat", 20);

			var font = BuildFontUsingPR34110Logic(span, defaultFont, 20);

			Assert.Equal("Arial", font.Family);
		}

		// =====================================================================
		// FontAttributes regression: PR #34110 drops Bold/Italic
		// =====================================================================

		[Fact]
		public void FontAttributes_Bold_DroppedByPR34110()
		{
			var span = new Span { Text = "Bold text", FontAttributes = FontAttributes.Bold };
			var defaultFont = Font.OfSize("DefaultFamily", 14);

			var font = BuildFontUsingPR34110Logic(span, defaultFont, 14);

			// PR logic produces Regular weight — Bold is lost
			Assert.Equal(FontWeight.Regular, font.Weight);
		}

		[Fact]
		public void FontAttributes_Bold_PreservedByCompleteLogic()
		{
			var span = new Span { Text = "Bold text", FontAttributes = FontAttributes.Bold };
			var defaultFont = Font.OfSize("DefaultFamily", 14);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 14);

			Assert.Equal(FontWeight.Bold, font.Weight);
		}

		[Fact]
		public void FontAttributes_Italic_DroppedByPR34110()
		{
			var span = new Span { Text = "Italic text", FontAttributes = FontAttributes.Italic };
			var defaultFont = Font.OfSize("DefaultFamily", 14);

			var font = BuildFontUsingPR34110Logic(span, defaultFont, 14);

			// PR logic produces Default slant — Italic is lost
			Assert.Equal(FontSlant.Default, font.Slant);
		}

		[Fact]
		public void FontAttributes_Italic_PreservedByCompleteLogic()
		{
			var span = new Span { Text = "Italic text", FontAttributes = FontAttributes.Italic };
			var defaultFont = Font.OfSize("DefaultFamily", 14);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 14);

			Assert.Equal(FontSlant.Italic, font.Slant);
		}

		[Fact]
		public void FontAttributes_InheritedFromDefaultFont_WhenNotSetOnSpan()
		{
			var span = new Span { Text = "Inherit bold" };
			var defaultFont = Font.OfSize("Montserrat", 20).WithAttributes(FontAttributes.Bold);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 20);

			Assert.Equal(FontWeight.Bold, font.Weight);
		}

		[Fact]
		public void FontAttributes_SpanOverridesDefaultFont_WhenExplicitlySet()
		{
			var span = new Span { Text = "Not bold", FontAttributes = FontAttributes.None };
			var defaultFont = Font.OfSize("Montserrat", 20).WithAttributes(FontAttributes.Bold);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 20);

			Assert.Equal(FontWeight.Regular, font.Weight);
		}

		// =====================================================================
		// FontAutoScalingEnabled regression: PR #34110 ignores the Span setting
		// =====================================================================

		[Fact]
		public void AutoScaling_Disabled_IgnoredByPR34110()
		{
			var span = new Span { Text = "Fixed size", FontAutoScalingEnabled = false, FontSize = 14 };
			var defaultFont = Font.OfSize("DefaultFamily", 14);

			var font = BuildFontUsingPR34110Logic(span, defaultFont, 14);

			// PR logic hardcodes AutoScalingEnabled to true
			Assert.True(font.AutoScalingEnabled);
		}

		[Fact]
		public void AutoScaling_Disabled_PreservedByCompleteLogic()
		{
			var span = new Span { Text = "Fixed size", FontAutoScalingEnabled = false, FontSize = 14 };
			var defaultFont = Font.OfSize("DefaultFamily", 14);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 14);

			Assert.False(font.AutoScalingEnabled);
		}

		[Fact]
		public void AutoScaling_InheritedFromDefaultFont_WhenNotSetOnSpan()
		{
			var span = new Span { Text = "Inherit scaling" };
			var defaultFont = Font.OfSize("Montserrat", 20, enableScaling: false);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 20);

			Assert.False(font.AutoScalingEnabled);
		}

		// =====================================================================
		// Font.IsDefault cascade: Bold-only Span becomes "default" and is skipped
		// =====================================================================

		[Fact]
		public void BoldOnlySpan_BecomesDefaultFont_WithPR34110()
		{
			// Span only sets Bold — no custom family or size
			var span = new Span { Text = "Just bold", FontAttributes = FontAttributes.Bold };

			// With PR logic: Font.OfSize(null, 14) with Regular weight
			// This may cause Font.IsDefault to be true, skipping the font span entirely
			var font = BuildFontUsingPR34110Logic(span, null, 14);

			Assert.Equal(FontWeight.Regular, font.Weight);
		}

		[Fact]
		public void BoldOnlySpan_IsNonDefault_WithCompleteLogic()
		{
			var span = new Span { Text = "Just bold", FontAttributes = FontAttributes.Bold };

			var font = BuildFontWithCompleteInheritance(span, null, 14);

			Assert.Equal(FontWeight.Bold, font.Weight);
			Assert.False(font.IsDefault);
		}

		// =====================================================================
		// Combined scenario: FontFamily inherited + Bold preserved
		// =====================================================================

		[Fact]
		public void Combined_FontFamilyInherited_BoldLost_WithPR34110()
		{
			// Label Style sets FontFamily="Montserrat", Span sets Bold
			var span = new Span { Text = "Bold Montserrat", FontAttributes = FontAttributes.Bold };
			var defaultFont = Font.OfSize("Montserrat", 20);

			var font = BuildFontUsingPR34110Logic(span, defaultFont, 20);

			// Family is inherited correctly
			Assert.Equal("Montserrat", font.Family);
			// But Bold is lost
			Assert.Equal(FontWeight.Regular, font.Weight);
		}

		[Fact]
		public void Combined_FontFamilyInherited_BoldPreserved_WithCompleteLogic()
		{
			var span = new Span { Text = "Bold Montserrat", FontAttributes = FontAttributes.Bold };
			var defaultFont = Font.OfSize("Montserrat", 20);

			var font = BuildFontWithCompleteInheritance(span, defaultFont, 20);

			Assert.Equal("Montserrat", font.Family);
			Assert.Equal(FontWeight.Bold, font.Weight);
		}
	}
}
