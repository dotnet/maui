using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz41296 : ContentPage
{
	public Bz41296()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void MarkupExtensionInDefaultNamespace(XamlInflator inflator)
		{
			var layout = new Bz41296(inflator);
			Assert.Equal("FooBar", layout.TestLabel.Text.ToString());
		}
	}
}
