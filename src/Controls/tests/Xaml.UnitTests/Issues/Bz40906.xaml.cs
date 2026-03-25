using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz40906 : ContentPage
{
	public Bz40906()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ParsingCDATA(XamlInflator inflator)
		{
			var page = new Bz40906(inflator);
			Assert.Equal("Foo", page.label0.Text);
			Assert.Equal("FooBar>><<", page.label1.Text);
		}
	}
}
