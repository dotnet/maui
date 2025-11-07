using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz43733Rd : ResourceDictionary
{
	public Bz43733Rd()
	{
		Add("SharedText", "Foo");
	}
}

public partial class Bz43733 : ContentPage
{
	public Bz43733()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void ThrowOnMissingDictionary(XamlInflator inflator)
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary
				{
					new Bz43733Rd()
				}
			};
			var p = new Bz43733(inflator);
			Assert.Equal("Foo", p.label.Text);
		}
	}
}
