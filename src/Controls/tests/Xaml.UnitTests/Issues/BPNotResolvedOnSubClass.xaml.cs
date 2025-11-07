using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BPNotResolvedOnSubClass : ContentPage
{
	public static readonly BindableProperty ShadowColorProperty =
		BindableProperty.CreateAttached("ShadowColor", typeof(Color), typeof(BPNotResolvedOnSubClass), null);

	public static Color GetShadowColor(Element bindable) // Change to Element instead of BindableObject o make fail
	{
		return (Color)bindable.GetValue(ShadowColorProperty);
	}

	public BPNotResolvedOnSubClass()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void CorrectlyResolveBPOnSubClasses(XamlInflator inflator)
		{
			var layout = new BPNotResolvedOnSubClass(inflator);
			var style = (Style)layout.Resources["Microsoft.Maui.Controls.Button"];
			Assert.NotNull(style);

			var button = new Button();
			button.Style = style;

			Assert.Equal(Color.FromArgb("#dddddd"), button.GetValue(ShadowColorProperty));
		}
	}
}
