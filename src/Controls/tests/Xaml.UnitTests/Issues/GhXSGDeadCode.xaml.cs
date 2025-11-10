using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GhXSGDeadCode : ContentPage
{
	public GhXSGDeadCode() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp] public void SetUp() => Application.Current = new MockApplication();

		[Test]
		public void SetterPropertyUsesCompiledConverter([Values] XamlInflator inflator)
		{
			var page = new GhXSGDeadCode(inflator);
			var style = (Style)page.Resources["testStyle"];
			
			Assert.NotNull(style);
			Assert.AreEqual(2, style.Setters.Count);
			
			var fontSizeSetter = style.Setters[0];
			Assert.AreEqual(Label.FontSizeProperty, fontSizeSetter.Property);
			Assert.AreEqual(16.0, fontSizeSetter.Value);
			
			var colorSetter = style.Setters[1];
			Assert.AreEqual(Label.TextColorProperty, colorSetter.Property);
			Assert.AreEqual(Colors.Red, colorSetter.Value);
		}

		[Test]
		public void SetterPropertyDoesNotGenerateDeadCode([Values(XamlInflator.SourceGen)] XamlInflator inflator)
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using Microsoft.Maui.Controls;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;
public partial class GhXSGDeadCode : ContentPage
{
	public GhXSGDeadCode() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(GhXSGDeadCode));
			
			Assert.IsFalse(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			
			// Save generated code for inspection
			System.IO.File.WriteAllText("/tmp/ghxsg_generated.cs", initComp);
			
			// The key improvement: Setter properties should not be set via assignment
			// (which would create service providers with XamlTypeResolver, etc.)
			// Instead, they should be initialized inline with the object initializer
			Assert.That(!initComp.Contains("setter.Property =", StringComparison.InvariantCulture), 
				"Setter.Property should not be set via assignment (dead code)");
			Assert.That(!initComp.Contains("setter.Value =", StringComparison.InvariantCulture),
				"Setter.Value should not be set via assignment (dead code)");
			Assert.That(!initComp.Contains("setter1.Property =", StringComparison.InvariantCulture),
				"Setter.Property should not be set via assignment (dead code)");
			Assert.That(!initComp.Contains("setter1.Value =", StringComparison.InvariantCulture),
				"Setter.Value should not be set via assignment (dead code)");
			
			// Should contain optimized Setter creation with inline initialization
			Assert.That(initComp.Contains("new global::Microsoft.Maui.Controls.Setter {Property = global::Microsoft.Maui.Controls.Label.FontSizeProperty, Value = 16D}", StringComparison.InvariantCulture),
				"Generated code should create Setter with inline initialization including compiled FontSizeProperty");
			Assert.That(initComp.Contains("new global::Microsoft.Maui.Controls.Setter {Property = global::Microsoft.Maui.Controls.Label.TextColorProperty, Value = global::Microsoft.Maui.Graphics.Colors.Red}", StringComparison.InvariantCulture),
				"Generated code should create Setter with inline initialization including compiled TextColorProperty");
		}
	}
}
