using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Bz45179_0 : ContentView
{
	public static int creator_count;
	public Bz45179_0()
	{
		creator_count++;
	}

}
public partial class Bz45179 : ContentPage
{
	public Bz45179()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void DTDoNotInstantiateTheirContent([Values] XamlInflator inflator)
		{
			Bz45179_0.creator_count = 0;
			Assume.That(Bz45179_0.creator_count, Is.EqualTo(0));
			var page = new Bz45179(inflator);
			Assert.That(Bz45179_0.creator_count, Is.EqualTo(0));
		}
	}
}