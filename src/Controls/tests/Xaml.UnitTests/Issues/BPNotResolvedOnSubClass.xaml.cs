﻿using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
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


	[TestFixture]
	class Tests
	{
		[Test]
		public void CorrectlyResolveBPOnSubClasses([Values]XamlInflator inflator)
		{
			var layout = new BPNotResolvedOnSubClass(inflator);
			var style = (Style)layout.Resources["Microsoft.Maui.Controls.Button"];
			Assert.NotNull(style);

			var button = new Button();
			button.Style = style;

			Assert.AreEqual(Color.FromArgb("#dddddd"), button.GetValue(ShadowColorProperty));
		}
	}
}
