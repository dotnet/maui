using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz41048 : ContentPage
{
	public Bz41048()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{
		public void Dispose()
		{
			Application.Current = null;
		}

		[Theory]
		[Values]
		public void StyleDoesNotOverrideValues(XamlInflator inflator)
		{
			var layout = new Bz41048(inflator);
			var label = layout.label0;
			Assert.Equal(Colors.Red, label.TextColor);
			Assert.Equal(FontAttributes.Bold, label.FontAttributes);
			Assert.Equal(LineBreakMode.WordWrap, label.LineBreakMode);
		}
	}
}