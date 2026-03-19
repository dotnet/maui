using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz31529 : ContentPage
	{
		public Bz31529()
		{
			InitializeComponent();
		}

		[Collection("Issue")]
		public class Tests
		{
			[Theory]
			[XamlInflatorData]
			internal void AllowWhiteSpacesInMarkups(XamlInflator inflator)
			{
				var layout = new Bz31529(inflator);
				Assert.Equal("Foo", layout.button.CommandParameter);
			}
		}
	}
}