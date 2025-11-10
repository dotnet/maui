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
			
			// The generated code should use compiled converters directly without creating service providers
			// Should NOT contain: XamlServiceProvider, SimpleValueTargetProvider, XmlNamespaceResolver, XamlTypeResolver
			Assert.That(!initComp.Contains("XamlServiceProvider", StringComparison.InvariantCulture), 
				"Generated code should not create XamlServiceProvider for compiled converters");
			Assert.That(!initComp.Contains("SimpleValueTargetProvider", StringComparison.InvariantCulture),
				"Generated code should not create SimpleValueTargetProvider for compiled converters");
			Assert.That(!initComp.Contains("XmlNamespaceResolver", StringComparison.InvariantCulture),
				"Generated code should not create XmlNamespaceResolver for compiled converters");
			Assert.That(!initComp.Contains("XamlTypeResolver", StringComparison.InvariantCulture),
				"Generated code should not create XamlTypeResolver for compiled converters");
			
			// Should contain optimized Setter creation
			Assert.That(initComp.Contains("new global::Microsoft.Maui.Controls.Setter", StringComparison.InvariantCulture),
				"Generated code should create Setters directly");
			Assert.That(initComp.Contains("Label.FontSizeProperty", StringComparison.InvariantCulture),
				"Generated code should reference FontSizeProperty directly");
		}
	}
}
