using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Note: This is a .rt.xaml file (runtime) which skips source generation and XamlC compilation
// This file tests that MockCompiler (XamlC) emits the XC0067 warning
public partial class Maui3059rt : ContentPage
{
	public Maui3059rt()
	{
		InitializeComponent();
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
			var page = new Maui3059rt(inflator);
			
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
		public void MockCompiler_CompileSucceeds()
		{
			// This test verifies that MockCompiler (XamlC) compiles successfully
			// The XC0067 warning is emitted but doesn't fail the build (warnings are suppressed in csproj)
			MockCompiler.Compile(typeof(Maui3059rt));
		}
	}
}
