using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui3059 : ContentPage
{
	public Maui3059()
	{
		InitializeComponent();
	}

	public Maui3059(XamlInflator inflator) : this()
	{
		// Constructor that accepts inflator for test compatibility
	}

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
		}

		[TearDown]
		public void TearDown()
		{
			Application.SetCurrentApplication(null);
		}

		[Test]
		public void BorderWithMultipleChildren_OnlyLastChildIsUsed([Values] XamlInflator inflator)
		{
			// This test verifies the behavior that only the last child is actually used
			// when multiple children are specified in a single-child content property
			var page = new Maui3059(inflator);
			
			Assert.IsNotNull(page.Content);
			Assert.IsInstanceOf<Microsoft.Maui.Controls.Border>(page.Content);
			
			var border = (Microsoft.Maui.Controls.Border)page.Content;
			Assert.IsNotNull(border.Content);
			Assert.IsInstanceOf<Label>(border.Content);
			
			var label = (Label)border.Content;
			// Only the last child ("Second") should be set
			Assert.AreEqual("Second", label.Text);
		}

		[Test]
		public void MockSourceGenerator_ReportsDuplicatePropertyWarning()
		{
			// Verify that MockSourceGenerator produces the MAUIX2006 warning
			MockCompiler.Compile(typeof(Maui3059), out var methodDef, out var diags, XamlInflator.SourceGen);
			
			// Check that the warning was reported
			var warnings = diags.Where(d => d.Severity == MockCompiler.DiagnosticSeverity.Warning).ToArray();
			Assert.IsNotEmpty(warnings, "Expected at least one warning from MockSourceGenerator");
			
			var duplicateWarning = warnings.FirstOrDefault(d => d.Id == "MAUIX2006");
			Assert.IsNotNull(duplicateWarning, "Expected MAUIX2006 warning from MockSourceGenerator");
			Assert.That(duplicateWarning.Message, Does.Contain("Border.Content"));
			Assert.That(duplicateWarning.Message, Does.Contain("multiple times"));
		}

		[Test]
		public void MockCompiler_ReportsDuplicatePropertyWarning()
		{
			// Verify that MockCompiler (XamlC) produces the XC0067 warning
			MockCompiler.Compile(typeof(Maui3059), out var methodDef, out var diags, XamlInflator.XamlC);
			
			// Check that the warning was reported
			var warnings = diags.Where(d => d.Severity == MockCompiler.DiagnosticSeverity.Warning).ToArray();
			Assert.IsNotEmpty(warnings, "Expected at least one warning from MockCompiler");
			
			var duplicateWarning = warnings.FirstOrDefault(d => d.Id == "XC0067");
			Assert.IsNotNull(duplicateWarning, "Expected XC0067 warning from MockCompiler");
			Assert.That(duplicateWarning.Message, Does.Contain("Border.Content"));
			Assert.That(duplicateWarning.Message, Does.Contain("multiple times"));
		}
	}
}
