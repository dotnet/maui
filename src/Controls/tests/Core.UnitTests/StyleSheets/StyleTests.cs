using System;
using System.Collections.Generic;
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

		[Fact]
		public void CssVariablesAreResolved()
		{
			var css = @":root { --primary: red; } label { color: var(--primary); }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			Assert.Equal(Colors.Red, (page.Content as Label).TextColor);
		}

		[Fact]
		public void CssVariablesWithFallback()
		{
			var css = @"label { color: var(--missing, blue); }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			Assert.Equal(Colors.Blue, (page.Content as Label).TextColor);
		}

		[Fact]
		public void CssVariablesReferenceOtherVariables()
		{
			var css = @":root { --base: green; --primary: var(--base); } label { color: var(--primary); }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			Assert.Equal(Colors.Green, (page.Content as Label).TextColor);
		}

		[Fact]
		public void CssVariablesAcrossStyleSheets()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(":root { --bg: blue; }"));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			// Add page-level sheet after LoadPage so that app variables are in scope
			page.Resources.Add(StyleSheet.FromString("label { background-color: var(--bg); }"));
			Assert.Equal(Colors.Blue, (page.Content as Label).BackgroundColor);
		}

		[Fact]
		public void CssVariablesChildOverridesParent()
		{
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(":root { --color: white; }"));
			var page = new ContentPage { Content = new Label() };
			page.Resources.Add(StyleSheet.FromString(":root { --color: red; } label { color: var(--color); }"));
			app.LoadPage(page);
			Assert.Equal(Colors.Red, (page.Content as Label).TextColor);
		}

		[Fact]
		public void CssVariablesDeclaredInNonRootRule()
		{
			var css = @".theme { --accent: purple; } label { color: var(--accent); }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			Assert.Equal(Colors.Purple, (page.Content as Label).TextColor);
		}

		[Fact]
		public void CssVariableCustomPropertyParsing()
		{
			// Verify --custom-name is parsed without errors
			var css = @":root { --my-custom-color: #FF5733; } label { color: var(--my-custom-color); }";
			var sheet = StyleSheet.FromString(css);
			Assert.NotNull(sheet.Variables);
			Assert.True(sheet.Variables.ContainsKey("--my-custom-color"));
			Assert.Equal("#FF5733", sheet.Variables["--my-custom-color"]);
		}

		// --- !important tests ---

		[Fact]
		public void ImportantDeclarationOverridesNormal()
		{
			// Two stylesheets: the first has !important, the second does not
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString("label { color: red !important; }"));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			// Apply a second stylesheet with a more specific selector (id) but without !important
			page.Resources.Add(StyleSheet.FromString("#myLabel { color: blue; }"));
			var label = (Label)page.Content;
			label.StyleId = "myLabel";
			app.LoadPage(page);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void ImportantDeclarationStrippedFromValue()
		{
			var styleString = @"background-color: #00ff00 !important;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.NotNull(style);
			Assert.Contains("background-color", (IEnumerable<string>)style.ImportantDeclarations);
			Assert.Equal("#00ff00", style.Declarations["background-color"]);
		}

		// --- calc() / rem / em tests ---

		[Fact]
		public void RemUnitConvertsToPixels()
		{
			// 1rem = 16px default; font-size: 2rem should be 32
			Assert.Equal("32", CssValueResolver.ResolveUnits("2rem"));
		}

		[Fact]
		public void EmUnitConvertsToPixels()
		{
			Assert.Equal("24", CssValueResolver.ResolveUnits("1.5em"));
		}

		[Fact]
		public void PxUnitPassesThrough()
		{
			Assert.Equal("20", CssValueResolver.ResolveUnits("20px"));
		}

		[Fact]
		public void CalcSimpleAddition()
		{
			Assert.Equal("36", CssValueResolver.ResolveUnits("calc(1rem + 20px)"));
		}

		[Fact]
		public void CalcSubtraction()
		{
			Assert.Equal("12", CssValueResolver.ResolveUnits("calc(2rem - 20)"));
		}

		[Fact]
		public void CalcMultiplication()
		{
			Assert.Equal("48", CssValueResolver.ResolveUnits("calc(3rem * 1)"));
		}

		[Fact]
		public void CalcPlainNumbersUnchanged()
		{
			// plain number without unit passes through unchanged
			Assert.Equal("42", CssValueResolver.ResolveUnits("42"));
		}

		[Fact]
		public void RemAppliedViaStylesheet()
		{
			// End-to-end: stylesheet with rem unit applies resolved value
			var css = @"label { font-size: 1.5rem; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var page = new ContentPage { Content = new Label() };
			app.LoadPage(page);
			Assert.Equal(24.0, (page.Content as Label).FontSize);
		}

		// --- New CSS property mapping tests ---

		[Fact]
		public void MaxHeightMapping()
		{
			var css = @"label { max-height: 100; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(100.0, label.MaximumHeightRequest);
		}

		[Fact]
		public void MaxWidthMapping()
		{
			var css = @"label { max-width: 200; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(200.0, label.MaximumWidthRequest);
		}

		[Fact]
		public void RotateMapping()
		{
			var css = @"label { rotate: 45; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(45.0, label.Rotation);
		}

		[Fact]
		public void ScaleMapping()
		{
			var css = @"label { scale: 2; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(2.0, label.Scale);
		}

		[Fact]
		public void ZIndexMapping()
		{
			var css = @"label { z-index: 5; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(5, label.ZIndex);
		}

		[Fact]
		public void FontWeightBoldMapping()
		{
			var css = @"label { font-weight: bold; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(FontAttributes.Bold, label.FontAttributes);
		}

		[Fact]
		public void GapMapsToSpacing()
		{
			var css = @"^layout { gap: 10; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var stack = new VerticalStackLayout();
			var page = new ContentPage { Content = stack };
			app.LoadPage(page);
			Assert.Equal(10.0, stack.Spacing);
		}

		[Fact]
		public void StrokeDashArrayMapping()
		{
			var css = @"border { stroke-dasharray: 4, 2; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var border = new Border();
			var page = new ContentPage { Content = border };
			app.LoadPage(page);
			Assert.NotNull(border.StrokeDashArray);
		}

		[Fact]
		public void OpacityWithRemUnit()
		{
			// opacity: 0.5 should still work (it's a plain double, not a unit)
			var css = @"label { opacity: 0.5; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(0.5, label.Opacity);
		}

		// --- Shorthand property expansion tests ---

		[Fact]
		public void BorderShorthandExpandsWidthAndColor()
		{
			var styleString = @"border: 2 solid red;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.Equal("2", style.Declarations["border-width"]);
			Assert.Equal("red", style.Declarations["border-color"]);
			Assert.False(style.Declarations.ContainsKey("border"));
		}

		[Fact]
		public void BorderShorthandWithHexColor()
		{
			var styleString = @"border: 1 #ff0000;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.Equal("1", style.Declarations["border-width"]);
			Assert.Equal("#ff0000", style.Declarations["border-color"]);
		}

		[Fact]
		public void FontShorthandExpandsAllProperties()
		{
			var styleString = @"font: italic bold 16 Arial;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.Equal("italic", style.Declarations["font-style"]);
			Assert.Equal("bold", style.Declarations["font-weight"]);
			Assert.Equal("16", style.Declarations["font-size"]);
			Assert.Equal("Arial", style.Declarations["font-family"]);
			Assert.False(style.Declarations.ContainsKey("font"));
		}

		[Fact]
		public void FontShorthandSizeAndFamilyOnly()
		{
			var styleString = @"font: 14 Helvetica;";
			var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
			Assert.Equal("14", style.Declarations["font-size"]);
			Assert.Equal("Helvetica", style.Declarations["font-family"]);
		}

		[Fact]
		public void BorderShorthandAppliedEndToEnd()
		{
			var css = @"button { border: 3 solid blue; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var button = new Button();
			var page = new ContentPage { Content = button };
			app.LoadPage(page);
			Assert.Equal(3.0, button.BorderWidth);
			Assert.Equal(Colors.Blue, button.BorderColor);
		}

		[Fact]
		public void FontShorthandAppliedEndToEnd()
		{
			var css = @"label { font: bold 20 sans-serif; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			Assert.Equal(FontAttributes.Bold, label.FontAttributes);
			Assert.Equal(20.0, label.FontSize);
			Assert.Equal("sans-serif", label.FontFamily);
		}

		// --- @import tests ---

		[Fact]
		public void ImportUrlParsed()
		{
			var css = @"@import url(""theme.css""); label { color: red; }";
			var sheet = StyleSheet.FromString(css);
			Assert.NotNull(sheet.Imports);
			Assert.Single(sheet.Imports);
			Assert.Equal("theme.css", sheet.Imports[0]);
		}

		[Fact]
		public void ImportStringParsed()
		{
			var css = @"@import ""base.css""; label { color: blue; }";
			var sheet = StyleSheet.FromString(css);
			Assert.NotNull(sheet.Imports);
			Assert.Equal("base.css", sheet.Imports[0]);
		}

		[Fact]
		public void MultipleImportsParsed()
		{
			var css = @"@import ""a.css""; @import url(""b.css""); label { color: green; }";
			var sheet = StyleSheet.FromString(css);
			Assert.Equal(2, sheet.Imports.Count);
			Assert.Equal("a.css", sheet.Imports[0]);
			Assert.Equal("b.css", sheet.Imports[1]);
		}

		[Fact]
		public void UnsupportedAtRulesSkipped()
		{
			// @media should be skipped without throwing
			var css = @"@media screen { label { color: red; } } label { color: blue; }";
			var sheet = StyleSheet.FromString(css);
			Assert.NotEmpty(sheet.Styles);
		}

		// --- inherit / unset keyword tests ---

		[Fact]
		public void InheritKeywordCopiesFromParent()
		{
			var css = @"^layout { background-color: red; } label { background-color: inherit; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			var stack = new VerticalStackLayout { Children = { label } };
			var page = new ContentPage { Content = stack };
			app.LoadPage(page);
			Assert.Equal(Colors.Red, label.BackgroundColor);
		}

		[Fact]
		public void UnsetKeywordClearsValue()
		{
			var app = new MockApplication();
			var label = new Label { BackgroundColor = Colors.Green };
			app.Resources.Add(StyleSheet.FromString("label { background-color: unset; }"));
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			// unset clears the value back to default
			Assert.Null(label.BackgroundColor);
		}

		[Fact]
		public void InitialKeywordResetsToDefault()
		{
			// First apply a color via CSS, then reset with initial
			var css = @"label { background-color: red; } label.reset { background-color: initial; }";
			var app = new MockApplication();
			app.Resources.Add(StyleSheet.FromString(css));
			var label = new Label();
			label.GetType().GetProperty("StyleClass")?.SetValue(label,
				new List<string> { "reset" });
			var page = new ContentPage { Content = label };
			app.LoadPage(page);
			// initial resets to the property default (null for BackgroundColor)
			Assert.Null(label.BackgroundColor);
		}

		[Fact]
		public void MediaQueryEvaluator_MinWidth_Matches()
		{
			Assert.True(MediaQueryEvaluator.Evaluate("(min-width: 768px)", 800, 600, ApplicationModel.AppTheme.Light));
			Assert.False(MediaQueryEvaluator.Evaluate("(min-width: 768px)", 500, 600, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void MediaQueryEvaluator_MaxWidth_Matches()
		{
			Assert.True(MediaQueryEvaluator.Evaluate("(max-width: 1200px)", 800, 600, ApplicationModel.AppTheme.Light));
			Assert.False(MediaQueryEvaluator.Evaluate("(max-width: 1200px)", 1400, 600, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void MediaQueryEvaluator_CompoundAnd()
		{
			Assert.True(MediaQueryEvaluator.Evaluate("(min-width: 576px) and (max-width: 768px)", 600, 600, ApplicationModel.AppTheme.Light));
			Assert.False(MediaQueryEvaluator.Evaluate("(min-width: 576px) and (max-width: 768px)", 900, 600, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void MediaQueryEvaluator_ScreenAndMinWidth()
		{
			Assert.True(MediaQueryEvaluator.Evaluate("screen and (min-width: 768px)", 800, 600, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void MediaQueryEvaluator_Orientation()
		{
			Assert.True(MediaQueryEvaluator.Evaluate("(orientation: portrait)", 400, 800, ApplicationModel.AppTheme.Light));
			Assert.True(MediaQueryEvaluator.Evaluate("(orientation: landscape)", 800, 400, ApplicationModel.AppTheme.Light));
			Assert.False(MediaQueryEvaluator.Evaluate("(orientation: landscape)", 400, 800, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void MediaQueryEvaluator_PrefersColorScheme()
		{
			Assert.True(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: dark)", 800, 600, ApplicationModel.AppTheme.Dark));
			Assert.False(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: dark)", 800, 600, ApplicationModel.AppTheme.Light));
			Assert.True(MediaQueryEvaluator.Evaluate("(prefers-color-scheme: light)", 800, 600, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void MediaQueryEvaluator_RemUnits()
		{
			// (min-width: 48rem) = (min-width: 768px)
			Assert.True(MediaQueryEvaluator.Evaluate("(min-width: 48rem)", 800, 600, ApplicationModel.AppTheme.Light));
			Assert.False(MediaQueryEvaluator.Evaluate("(min-width: 48rem)", 500, 600, ApplicationModel.AppTheme.Light));
		}

		[Fact]
		public void StyleSheet_MediaGroups_ActiveWhenConditionMet()
		{
			var sheet = StyleSheet.CreateCompiled(
				new CompiledCssRule[]
				{
					new CompiledCssRule("label", new[] { new KeyValuePair<string, string>("color", "red") })
				},
				new CompiledCssMediaGroup[]
				{
					new CompiledCssMediaGroup("(min-width: 768px)", new CompiledCssRule[]
					{
						new CompiledCssRule("label", new[] { new KeyValuePair<string, string>("color", "blue") })
					})
				});

			// Initially no media groups active
			Assert.NotNull(sheet.MediaGroups);
			Assert.Single(sheet.MediaGroups);
			Assert.False(sheet.MediaGroups[0].IsActive);

			// Evaluate with wide window → group becomes active
			bool changed = sheet.EvaluateMediaQueries(1024, 768, ApplicationModel.AppTheme.Light);
			Assert.True(changed);
			Assert.True(sheet.MediaGroups[0].IsActive);

			// Evaluate with narrow window → group becomes inactive
			changed = sheet.EvaluateMediaQueries(500, 768, ApplicationModel.AppTheme.Light);
			Assert.True(changed);
			Assert.False(sheet.MediaGroups[0].IsActive);

			// Evaluate again with same narrow window → no change
			changed = sheet.EvaluateMediaQueries(500, 768, ApplicationModel.AppTheme.Light);
			Assert.False(changed);
		}

		[Fact]
		public void RuntimeParsing_MediaQuery_ParsesGroups()
		{
			var css = @"
label { color: red; }
@media (min-width: 768px) {
  label { color: blue; }
  .big { font-size: 24; }
}
@charset ""UTF-8"";
";
			var sheet = StyleSheet.FromString(css);
			Assert.NotNull(sheet.MediaGroups);
			Assert.Single(sheet.MediaGroups);
			Assert.Contains("min-width: 768px", sheet.MediaGroups[0].Condition, StringComparison.Ordinal);
			Assert.Equal(2, sheet.MediaGroups[0].Styles.Count);
		}

		[Fact]
		public void RuntimeParsing_MediaQuery_AppliesWhenActive()
		{
			var css = @"
label { color: red; }
@media (min-width: 768px) {
  label { color: blue; }
}
";
			var app = new MockApplication();
			var sheet = StyleSheet.FromString(css);
			app.Resources.Add(sheet);

			var label = new Label();
			var page = new ContentPage { Content = label };
			app.LoadPage(page);

			// Base style applied (red) — media group not yet evaluated
			Assert.Equal(Colors.Red, label.TextColor);

			// Evaluate with wide window → media group becomes active
			sheet.EvaluateMediaQueries(1024, 768, ApplicationModel.AppTheme.Light);
			// Re-apply to see the override
			sheet.Apply(label, null);

			Assert.Equal(Colors.Blue, label.TextColor);
		}

	}
}
[Test]
public void PseudoClassHover()
{
var label = new Label { AutomationId = "label" };
var app = new MockApplication();
app.Resources.Add(new StyleSheet
{
Rules =
{
new Selector { new Selector.Class("my-hover") { VisibilityConverter = c => new Selector.Class(c.Substring(4)).Matches(label) } }.Matches(label)
? new StyleRule { Selector = ".my-hover:hover", Declaration = "color: red" }
: null,
}.OfType<StyleRule>().ToList()
}.AsReadOnly());

app.LoadPage(label);
// Verify that :hover pseudo-class creates a VisualStateManager state
var vsm = VisualStateManager.GetVisualStateGroups(label);
Assert.That(vsm, Is.Not.Null);
}
