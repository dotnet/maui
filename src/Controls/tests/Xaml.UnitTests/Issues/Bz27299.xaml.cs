using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz27299ViewModel
{
	public string Text
	{
		get { return "Foo"; }
	}
}
public class Bz27299ViewModelLocator
{
	public static int Count { get; set; }
	public object Bz27299
	{
		get
		{
			Count++;
			return new Bz27299ViewModel();
		}
	}
}

public partial class Bz27299 : ContentPage
{
	public Bz27299()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests
	{
		public Tests()
		{
			Bz27299ViewModelLocator.Count = 0;
		}

		[Theory]
		[XamlInflatorData]
		internal void ViewModelLocatorOnlyCalledOnce(XamlInflator inflator)
		{
			Assert.Equal(0, Bz27299ViewModelLocator.Count);
			var layout = new Bz27299(inflator);
			Assert.Equal(1, Bz27299ViewModelLocator.Count);
			Assert.Equal("Foo", layout.label.Text);
		}
	}
}