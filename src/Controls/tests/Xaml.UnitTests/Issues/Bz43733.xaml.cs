using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void ThrowOnMissingDictionary([Values] XamlInflator inflator)
		{
			Application.Current = new MockApplication
			{
				Resources = new ResourceDictionary
				{
					new Bz43733Rd()
				}
			};
			var p = new Bz43733(inflator);
			Assert.AreEqual("Foo", p.label.Text);
		}
	}
}
