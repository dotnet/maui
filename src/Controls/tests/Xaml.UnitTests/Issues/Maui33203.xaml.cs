using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33203 : ContentPage
{
	public Maui33203() => InitializeComponent();

	public Maui33203(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => Application.Current = new MockApplication();
		public void Dispose() => Application.Current = null;

		[Theory]
		[XamlInflatorData]
		internal void ImplicitStyleWithBasedOnKeyedStyleWorks(XamlInflator inflator)
		{
			// This test reproduces issue #33203 where implicit styles using BasedOn
			// with keyed styles stopped working in 10.0.20
			var page = new Maui33203(inflator);
			Application.Current.LoadPage(page);

			// CustomLabel should have Blue background from the LabelBase keyed style
			Assert.Equal(Colors.Blue, page.baseLabel.BackgroundColor);
			Assert.Equal(20, page.baseLabel.FontSize);
		}

		[Theory]
		[XamlInflatorData]
		internal void DerivedImplicitStyleOverridesBaseStyle(XamlInflator inflator)
		{
			// This test verifies that the more specific DerivedLabel style
			// overrides the base CustomLabel style's BackgroundColor
			var page = new Maui33203(inflator);
			Application.Current.LoadPage(page);

			// DerivedLabel should have Red background from DerivedLabelStyle keyed style
			// NOT Blue from LabelBase - the derived style should override
			Assert.Equal(Colors.Red, page.derivedLabel.BackgroundColor);
			// FontSize should still be inherited from base style
			Assert.Equal(20, page.derivedLabel.FontSize);
		}
	}
}

// Custom Label classes for testing keyed+BasedOn implicit style scenarios
public class CustomLabel33203 : Label
{
}

public class DerivedLabel33203 : CustomLabel33203
{
}
